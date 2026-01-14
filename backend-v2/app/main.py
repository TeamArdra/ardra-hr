from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.database.connection import connect_to_mongo, close_mongo_connection, db
from app.routers import auth
from app.routers import reviews
import asyncio
from app.routers.reviews import cleanup_old_reviews
from app.config import settings
import datetime

app = FastAPI(title="VIT Auth API", version="1.0.0")

# CORS configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000", "http://localhost:5173"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Event handlers
@app.on_event("startup")
async def startup_db_client():
    await connect_to_mongo()
    # Ensure unique index on registration number to prevent duplicate accounts
    database = db.client[settings.database_name]
    users = database["users"]
    # create_index is idempotent; if index exists it's a no-op
    await users.create_index("regNumber", unique=True)
    # Ensure index for reviews querying
    reviews_coll = database["reviews"]
    await reviews_coll.create_index([("subjectRegNumber", 1), ("month_year", 1)])

    # Start background monthly cleanup task
    async def _monthly_worker():
        try:
            while True:
                # cleanup immediately once on startup, then wait until next month
                await cleanup_old_reviews(db.client, settings.database_name)
                now = datetime.utcnow()
                # compute first day of next month at 00:00 UTC
                year = now.year + (1 if now.month == 12 else 0)
                month = 1 if now.month == 12 else now.month + 1
                next_month = datetime(year, month, 1)
                sleep_seconds = (next_month - now).total_seconds()
                await asyncio.sleep(sleep_seconds)
        except asyncio.CancelledError:
            return

    app.state._monthly_cleanup_task = asyncio.create_task(_monthly_worker())

@app.on_event("shutdown")
async def shutdown_db_client():
    await close_mongo_connection()
    # cancel monthly cleanup if running
    task = getattr(app.state, "_monthly_cleanup_task", None)
    if task:
        task.cancel()

# Include routers
app.include_router(auth.router)
app.include_router(reviews.router)

@app.get("/")
async def root():
    return {"message": "VIT Auth API is running"}