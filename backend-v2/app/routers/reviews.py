from fastapi import APIRouter, HTTPException, Depends, status
from app.schemas.review import ReviewCreate
from app.database.connection import get_database
from datetime import datetime
from typing import List
from bson import ObjectId

router = APIRouter(prefix="/api/reviews", tags=["reviews"])

def _current_month_year() -> str:
    now = datetime.utcnow()
    return f"{now.year:04d}-{now.month:02d}"


@router.post("/", status_code=status.HTTP_201_CREATED)
async def create_review(payload: ReviewCreate, db=Depends(get_database)):
    users = db["users"]
    reviews = db["reviews"]

    # Optional: verify reviewer and subject exist
    reviewer = await users.find_one({"regNumber": payload.reviewer_reg_number})
    subject = await users.find_one({"regNumber": payload.subject_reg_number})
    if not reviewer or not subject:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Reviewer or subject not found")

    month_year = _current_month_year()
    doc = {
        "reviewerRegNumber": payload.reviewer_reg_number,
        "subjectRegNumber": payload.subject_reg_number,
        "content": payload.content,
        "rating": int(payload.rating),
        "month_year": month_year,
        "created_at": datetime.utcnow()
    }

    res = await reviews.insert_one(doc)
    return {"id": str(res.inserted_id)}


@router.get("/", status_code=status.HTTP_200_OK)
async def get_reviews(subjectRegNumber: str | None = None, db=Depends(get_database)):
    """
    Get reviews for a subject reg number. If `subjectRegNumber` is omitted,
    returns an empty list (require explicit reg number).
    Only returns reviews for the current month.
    """
    if not subjectRegNumber:
        return []

    reviews = db["reviews"]
    month_year = _current_month_year()
    cursor = reviews.find({"subjectRegNumber": subjectRegNumber, "month_year": month_year})
    results = []
    async for r in cursor:
        results.append({
            "id": str(r.get("_id")),
            "reviewer_reg_number": r.get("reviewerRegNumber"),
            "subject_reg_number": r.get("subjectRegNumber"),
            "content": r.get("content"),
            "rating": r.get("rating"),
            "month_year": r.get("month_year"),
            "created_at": r.get("created_at").isoformat() if r.get("created_at") else None
        })

    return results


@router.get("/people", status_code=status.HTTP_200_OK)
async def list_people(excludeRegNumber: str | None = None, db=Depends(get_database)):
    """
    Return list of users (name and regNumber) excluding the provided reg number.
    """
    users = db["users"]
    query = {}
    if excludeRegNumber:
        query = {"regNumber": {"$ne": excludeRegNumber}}

    cursor = users.find(query, {"name": 1, "regNumber": 1, "_id": 0})
    results = []
    async for u in cursor:
        results.append({
            "name": u.get("name"),
            "regNumber": u.get("regNumber")
        })

    return results


async def cleanup_old_reviews(db_client, database_name: str):
    database = db_client[database_name]
    reviews = database["reviews"]
    current = _current_month_year()
    await reviews.delete_many({"month_year": {"$ne": current}})
