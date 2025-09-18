📌 Tbilink Backend

This is the backend service for the Tbilink Social Media project, built with ASP.NET Core Web API and MS SQL Server.
It provides all API endpoints for authentication, user management, posts, comments, and other social features.

🚀 Tech Stack

ASP.NET Core Web API (C#)

Entity Framework Core

MS SQL Server

Swagger / Swashbuckle for API documentation

⚙️ Getting Started
1. Clone the Repository
git clone https://github.com/levanmartirosyan/Tbilink-Back.git
cd Tbilink-Back

2. Configure Database

Update the connection string in appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=TbilinkDB;Trusted_Connection=True;TrustServerCertificate=true"
}


Apply migrations:

dotnet ef database update

3. Run the API
dotnet run


Backend will be available at:

http://localhost:5136

📖 Swagger API Docs

Swagger UI is enabled in development mode.
Once the API is running, visit:

👉 http://localhost:5136/swagger

Here you can test all endpoints directly.

📂 Project Structure
Tbilink-Back/
│── Controllers/      # API controllers
│── Models/           # Entity models
│── Data/             # DbContext and migrations
│── Services/         # Business logic
│── DTOs/             # Data transfer objects
│── Program.cs        # App entry point
│── appsettings.json  # Configurations

🧪 Testing

You can use Swagger UI or tools like Postman to test the API endpoints.

Example:

POST /api/auth/register
POST /api/auth/login
GET  /api/users/{id}
