# 🌏 Vntourland - Custom Tour Booking Platform

> A comprehensive travel marketplace connecting travelers with local sellers to create personalized tour experiences.

## 📖 Introduction

**Vntourland** is a web-based tourism platform built with **ASP.NET Core Razor Pages**. Unlike traditional booking sites, Vntourland focuses on **Custom Tours**, allowing users to send specific travel requirements (budget, destination, interests) to verified Sellers/Agencies, who then bid or propose tailored itineraries.

The system is architected using the **3-Layer Pattern (N-Tier)** to ensure clean separation between the User Interface, Business Logic, and Data Access.

## 🏗️ Architecture & Design Pattern

The solution follows a strict **3-Layer Architecture**:

1.  **Presentation Layer (`Vntourland.WebApp`):**
    * Built with **Razor Pages** (MVVM pattern).
    * Uses **Tag Helpers** and **View Components** for reusable UI.
    * Responsive design with Bootstrap 5.
2.  **Business Logic Layer (`Vntourland.BLL`):**
    * Handles core logic: Price calculation, Tour customization rules, Booking validation.
    * Services: `TourService`, `BookingService`.
3.  **Data Access Layer (`Vntourland.DAL`):**
    * Manages database interactions using **Entity Framework Core**.
    * Repository Pattern implementation.
4.  **Common/Core:** Shared DTOs, Enums, and Constants.

## 🛠️ Tech Stack

* **Framework:** .NET 8
* **Technology:** ASP.NET Core Razor Pages
* **Database:** SQL Server
* **ORM:** Entity Framework Core
* **Frontend:** HTML5, CSS3, Bootstrap 5, jQuery (for AJAX interactions)
* **Real-time:** SignalR (for Chat between User & Seller)
* **Authentication:** JWT (JSON Web Tokens)


