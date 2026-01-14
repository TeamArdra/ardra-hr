from pydantic import BaseModel, Field
from typing import Optional

class ReviewCreate(BaseModel):
    reviewer_reg_number: str = Field(..., alias="reviewerRegNumber")
    subject_reg_number: str = Field(..., alias="subjectRegNumber")
    content: str
    rating: int

    class Config:
        populate_by_name = True

class ReviewOut(BaseModel):
    id: str
    reviewer_reg_number: str
    subject_reg_number: str
    content: str
    rating: int
    month_year: str
    created_at: str

    class Config:
        populate_by_name = True
