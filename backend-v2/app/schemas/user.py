from pydantic import BaseModel, EmailStr, Field
from typing import Optional

class UserSignup(BaseModel):
    name: str
    reg_number: str = Field(..., alias="regNumber")
    mobile: str
    vit_email: EmailStr = Field(..., alias="vitEmail")
    personal_email: EmailStr = Field(..., alias="personalEmail")
    team_number: str = Field(..., alias="teamNumber")
    codename: str
    password: str
    residence_type: str = Field(..., alias="residenceType")
    hostel_type: Optional[str] = Field(None, alias="hostelType")
    block_room: Optional[str] = Field(None, alias="blockRoom")
    
    class Config:
        populate_by_name = True

class UserLogin(BaseModel):
    reg_number: str = Field(..., alias="regNumber")
    password: str
    
    class Config:
        populate_by_name = True

class Token(BaseModel):
    access_token: str
    token_type: str = "bearer"
    token: str  # For frontend compatibility