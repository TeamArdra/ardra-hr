from fastapi import APIRouter, HTTPException, Depends, status
from app.schemas.user import UserSignup, UserLogin, Token
from app.models.user import User
from app.database.connection import get_database
from datetime import datetime, timedelta
from app.config import settings

# Simple, self-contained security helpers (inline to keep router simple)
import base64
import hashlib
import hmac
import json
import os

# PBKDF2 settings
_PBKDF2_ITER = 100_000
_SALT_BYTES = 16

def hash_password(password: str) -> str:
    salt = os.urandom(_SALT_BYTES)
    dk = hashlib.pbkdf2_hmac("sha256", password.encode("utf-8"), salt, _PBKDF2_ITER)
    salt_b64 = base64.urlsafe_b64encode(salt).rstrip(b"=").decode("utf-8")
    dk_b64 = base64.urlsafe_b64encode(dk).rstrip(b"=").decode("utf-8")
    return f"pbkdf2_sha256${_PBKDF2_ITER}${salt_b64}${dk_b64}"

def verify_password(plain_password: str, hashed_password: str) -> bool:
    try:
        parts = hashed_password.split("$")
        if len(parts) != 4:
            return False
        algo, iter_s, salt_b64, dk_b64 = parts
        iterations = int(iter_s)
        # Restore base64 padding to a multiple of 4
        def _pad(s: str) -> str:
            return s + "=" * (-len(s) % 4)
        salt = base64.urlsafe_b64decode(_pad(salt_b64))
        expected_dk = base64.urlsafe_b64decode(_pad(dk_b64))
    except Exception:
        return False

    computed = hashlib.pbkdf2_hmac("sha256", plain_password.encode("utf-8"), salt, iterations)
    return hmac.compare_digest(computed, expected_dk)

def _base64url_encode(data: bytes) -> str:
    return base64.urlsafe_b64encode(data).rstrip(b"=").decode("utf-8")

def _to_uint_seconds(dt: datetime) -> int:
    return int(dt.timestamp())

def create_access_token(data: dict, expires_delta: timedelta | None = None) -> str:
    header = {"alg": settings.algorithm, "typ": "JWT"}
    payload = data.copy()
    expire_time = datetime.utcnow() + (expires_delta if expires_delta else timedelta(minutes=settings.access_token_expire_minutes))
    payload.update({"exp": _to_uint_seconds(expire_time)})

    header_b64 = _base64url_encode(json.dumps(header, separators=(",", ":")).encode("utf-8"))
    payload_b64 = _base64url_encode(json.dumps(payload, separators=(",", ":")).encode("utf-8"))

    signing_input = f"{header_b64}.{payload_b64}".encode("utf-8")
    key = settings.secret_key.encode("utf-8")

    if settings.algorithm.upper().startswith("HS"):
        hash_name = "sha256" if settings.algorithm.upper() == "HS256" else "sha384" if settings.algorithm.upper() == "HS384" else "sha512"
        signature = hmac.new(key, signing_input, getattr(hashlib, hash_name)).digest()
    else:
        raise ValueError("Unsupported algorithm for manual JWT creation")

    signature_b64 = _base64url_encode(signature)
    return f"{header_b64}.{payload_b64}.{signature_b64}"

router = APIRouter(prefix="/api/auth", tags=["auth"])

@router.post("/signup", response_model=Token)
async def signup(user: UserSignup, db = Depends(get_database)):
    users_collection = db["users"]
    
    # Check if user already exists
    existing_user = await users_collection.find_one({
        "$or": [
            {"vit_email": user.vit_email},
            {"reg_number": user.reg_number}
        ]
    })
    
    if existing_user:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="User with this email or registration number already exists"
        )
    
    # Hash the password and store hashed value in the model's `password` field
    hashed_password = hash_password(user.password)

    # Create user document (replace plain password with hashed)
    user_dict = user.model_dump(by_alias=True)
    user_dict["password"] = hashed_password
    
    user_in_db = User(**user_dict)
    
    # Insert into database
    result = await users_collection.insert_one(user_in_db.model_dump(by_alias=True))
    
    # Create access token
    access_token = create_access_token(
        data={"sub": user.vit_email, "reg_number": user.reg_number},
        expires_delta=timedelta(minutes=settings.access_token_expire_minutes)
    )
    
    return {
        "access_token": access_token,
        "token_type": "bearer",
        "token": access_token
    }

@router.post("/login", response_model=Token)
async def login(credentials: UserLogin, db = Depends(get_database)):
    users_collection = db["users"]

    # Find user by registration number (DB stores aliased field names)
    user = await users_collection.find_one({
        "regNumber": credentials.reg_number
    })
    
    if not user:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid email or password"
        )
    
    # Verify password (stored in `password` field)
    if not verify_password(credentials.password, user["password"]):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid email or password"
        )
    
    # Create access token (use registration number as subject)
    access_token = create_access_token(
        data={"sub": credentials.reg_number, "reg_number": user.get("regNumber")},
        expires_delta=timedelta(minutes=settings.access_token_expire_minutes)
    )
    
    return {
        "access_token": access_token,
        "token_type": "bearer",
        "token": access_token
    }