-- ============================================================
-- Database: RaceDayDB
-- Description: Full database schema for RaceDay Event Management System
-- ============================================================

USE master;
GO

-- Drop database if it exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'RaceDayDB')
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

-- Create database
CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- ============================================================
-- Table: Users
-- ============================================================
-- Created Users table to store login credentials and users types
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    UserType NVARCHAR(20) NOT NULL CHECK (UserType IN ('Organiser', 'Participant')),
    DateRegistered DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- ============================================================
-- Table: Events
-- ============================================================
-- Created Events table to store race details like date , location , and organiser reference
CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    EventName NVARCHAR(100) NOT NULL,
    EventDescription NVARCHAR(MAX) NULL,
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Venue NVARCHAR(255) NOT NULL,
    City NVARCHAR(50) NOT NULL,
    Province NVARCHAR(50) NOT NULL,
    MaxParticipants INT NOT NULL DEFAULT 100,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- Table: Categories
-- ============================================================
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    CategoryName NVARCHAR(50) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0,
    MaxParticipants INT NOT NULL DEFAULT 50
);
GO

-- ============================================================
-- Table: Enrolments
-- ============================================================
CREATE TABLE Enrolments (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Waitlisted')),
    BibNumber NVARCHAR(20) NULL UNIQUE
);
GO

-- ============================================================
-- Table: Results
-- ============================================================
CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId INT NOT NULL UNIQUE,
    FinishTime TIME NULL,
    OverallPosition INT NULL,
    CategoryPosition INT NULL,
    Pace DECIMAL(5,2) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'DNS' CHECK (Status IN ('DNS', 'DNF', 'FINISHED', 'DISQUALIFIED')),
    RecordedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- Table: AuditLog
-- ============================================================
CREATE TABLE AuditLog (
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    TableName NVARCHAR(100) NOT NULL,
    RecordId INT NOT NULL,
    Action NVARCHAR(20) NOT NULL CHECK (Action IN ('INSERT', 'UPDATE', 'DELETE')),
    UserId INT NULL,
    ChangeData NVARCHAR(MAX) NULL,
    ChangeDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- ADD FOREIGN KEY CONSTRAINTS
-- ============================================================
ALTER TABLE Events ADD CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) REFERENCES Users(UserId);
ALTER TABLE Categories ADD CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE;
ALTER TABLE Enrolments ADD CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantId) REFERENCES Users(UserId);
ALTER TABLE Enrolments ADD CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventId) REFERENCES Events(EventId);
ALTER TABLE Enrolments ADD CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId);
ALTER TABLE Results ADD CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId) REFERENCES Enrolments(EnrolmentId);
ALTER TABLE AuditLog ADD CONSTRAINT FK_AuditLog_Users FOREIGN KEY (UserId) REFERENCES Users(UserId);
GO

-- ============================================================
-- INSERT SAMPLE DATA
-- ============================================================

-- Insert Organisers
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, UserType)
VALUES 
    ('organiser1@raceday.co.za', 'hash123', 'Thabo', 'Mokoena', 'Organiser'),
    ('organiser2@raceday.co.za', 'hash456', 'Sarah', 'Van der Merwe', 'Organiser');
GO

-- Insert Participants
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, UserType)
VALUES 
    ('participant1@email.com', 'hash789', 'John', 'Doe', 'Participant'),
    ('participant2@email.com', 'hash101', 'Jane', 'Smith', 'Participant');
GO

-- Insert Events
INSERT INTO Events (OrganiserId, EventName, EventDescription, EventDate, StartTime, EndTime, Venue, City, Province, MaxParticipants)
VALUES 
    (1, 'Cape Town Cycle Tour', 'Annual cycle tour through Cape Town', '2026-03-08', '06:00:00', '18:00:00', 'Cape Town Stadium', 'Cape Town', 'Western Cape', 5000),
    (2, 'Soweto Marathon', 'Iconic marathon through Soweto', '2026-11-01', '05:30:00', '13:00:00', 'FNB Stadium', 'Johannesburg', 'Gauteng', 3000),
    (1, 'Durban Park Run', 'Community 5km park run', '2026-02-15', '07:00:00', '09:00:00', 'Durban Botanical Gardens', 'Durban', 'KwaZulu-Natal', 500);
GO

-- Insert Categories
INSERT INTO Categories (EventId, CategoryName, Distance, EntryFee, MaxParticipants)
VALUES 
    (1, 'Elite Men', 109.00, 350.00, 200),
    (1, 'Elite Women', 109.00, 350.00, 100),
    (1, 'Recreational', 109.00, 250.00, 4700),
    (2, 'Full Marathon', 42.20, 250.00, 1000),
    (2, 'Half Marathon', 21.10, 150.00, 1000),
    (2, '10km Run', 10.00, 80.00, 1000),
    (3, '5km Walk', 5.00, 0.00, 250),
    (3, '5km Run', 5.00, 0.00, 250);
GO

-- Insert Enrolments
INSERT INTO Enrolments (ParticipantId, EventId, CategoryId, Status, BibNumber)
VALUES 
    (3, 1, 3, 'Confirmed', 'CT-1001'),
    (4, 1, 2, 'Confirmed', 'CT-1002'),
    (3, 2, 5, 'Pending', 'SM-2001'),
    (4, 2, 4, 'Confirmed', 'SM-2002'),
    (3, 3, 7, 'Confirmed', 'DB-3001'),
    (4, 3, 6, 'Confirmed', 'DB-3002');
GO

-- Insert Results
INSERT INTO Results (EnrolmentId, FinishTime, OverallPosition, CategoryPosition, Pace, Status)
VALUES 
    (1, '03:45:22', 150, 45, 5.17, 'FINISHED'),
    (2, '03:12:45', 25, 2, 4.58, 'FINISHED'),
    (4, '04:20:15', 320, 85, 6.10, 'FINISHED'),
    (5, '00:28:30', 45, 20, 5.42, 'FINISHED');
GO

-- ============================================================
-- Verification Queries
-- ============================================================

SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'Events', COUNT(*) FROM Events
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL
SELECT 'Enrolments', COUNT(*) FROM Enrolments
UNION ALL
SELECT 'Results', COUNT(*) FROM Results;
GO

SELECT 
    u.FirstName + ' ' + u.LastName AS Participant,
    e.EventName,
    c.CategoryName,
    en.Status,
    r.FinishTime,
    r.OverallPosition
FROM Enrolments en
JOIN Users u ON en.ParticipantId = u.UserId
JOIN Events e ON en.EventId = e.EventId
JOIN Categories c ON en.CategoryId = c.CategoryId
LEFT JOIN Results r ON en.EnrolmentId = r.EnrolmentId
WHERE u.UserType = 'Participant'
ORDER BY en.EnrolmentId;
GO

-- ============================================================
-- END OF SCRIPT
-- ============================================================