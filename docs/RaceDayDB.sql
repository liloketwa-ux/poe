/*
    RaceDay Database Script
    Part 1 - System Planning and Database
    SQL Server / SSMS

    This script can be run on a clean SQL Server instance.
    It creates RaceDayDB, all tables/constraints, and realistic seed data.
*/

IF DB_ID(N'RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

/* Make the script repeatable during development. */
IF OBJECT_ID(N'dbo.Results', N'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID(N'dbo.Enrolments', N'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID(N'dbo.Categories', N'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID(N'dbo.Routes', N'U') IS NOT NULL DROP TABLE dbo.Routes;
IF OBJECT_ID(N'dbo.Events', N'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) NOT NULL,
    FirstName NVARCHAR(60) NOT NULL,
    LastName NVARCHAR(60) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    Phone NVARCHAR(30) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,

    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'Organiser', N'Participant'))
);
GO

CREATE TABLE dbo.Events
(
    EventId INT IDENTITY(1,1) NOT NULL,
    OrganiserId INT NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,
    EventDate DATE NOT NULL,
    RegistrationOpen DATE NOT NULL,
    RegistrationClose DATE NOT NULL,
    Venue NVARCHAR(150) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    Province NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EventType NVARCHAR(20) NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Events_Status DEFAULT N'Open',

    CONSTRAINT PK_Events PRIMARY KEY (EventId),
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Events_Distance CHECK (DistanceKm > 0),
    CONSTRAINT CK_Events_Type CHECK (EventType IN (N'Running', N'Walking', N'Cycling')),
    CONSTRAINT CK_Events_Status CHECK (Status IN (N'Draft', N'Open', N'Closed', N'Completed', N'Cancelled')),
    CONSTRAINT CK_Events_RegistrationDates CHECK (RegistrationClose >= RegistrationOpen),
    CONSTRAINT CK_Events_EventAfterRegistration CHECK (EventDate >= RegistrationOpen)
);
GO

CREATE TABLE dbo.Routes
(
    RouteId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    StartLocation NVARCHAR(200) NOT NULL,
    FinishLocation NVARCHAR(200) NOT NULL,
    RouteUrl NVARCHAR(500) NULL,
    ElevationGainM INT NULL,
    Notes NVARCHAR(500) NULL,

    CONSTRAINT PK_Routes PRIMARY KEY (RouteId),
    CONSTRAINT UQ_Routes_Event UNIQUE (EventId),
    CONSTRAINT FK_Routes_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId),
    CONSTRAINT CK_Routes_Elevation CHECK (ElevationGainM IS NULL OR ElevationGainM >= 0)
);
GO

CREATE TABLE dbo.Categories
(
    CategoryId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,
    MaxEntries INT NOT NULL,
    MinAge INT NULL,
    MaxAge INT NULL,

    CONSTRAINT PK_Categories PRIMARY KEY (CategoryId),
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId),
    CONSTRAINT UQ_Categories_Event_Name UNIQUE (EventId, Name),
    CONSTRAINT CK_Categories_Distance CHECK (DistanceKm > 0),
    CONSTRAINT CK_Categories_EntryFee CHECK (EntryFee >= 0),
    CONSTRAINT CK_Categories_MaxEntries CHECK (MaxEntries > 0),
    CONSTRAINT CK_Categories_Ages CHECK (
        (MinAge IS NULL AND MaxAge IS NULL)
        OR (MinAge IS NOT NULL AND MaxAge IS NOT NULL AND MinAge >= 0 AND MaxAge >= MinAge)
    )
);
GO

CREATE TABLE dbo.Enrolments
(
    EnrolmentId INT IDENTITY(1,1) NOT NULL,
    ParticipantId INT NOT NULL,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolmentDate DATETIME2(0) NOT NULL CONSTRAINT DF_Enrolments_EnrolmentDate DEFAULT SYSUTCDATETIME(),
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Enrolments_Status DEFAULT N'Confirmed',

    CONSTRAINT PK_Enrolments PRIMARY KEY (EnrolmentId),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Enrolments_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId),
    CONSTRAINT FK_Enrolments_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT UQ_Enrolments_Participant_Event UNIQUE (ParticipantId, EventId),
    CONSTRAINT CK_Enrolments_Status CHECK (Status IN (N'Confirmed', N'Cancelled'))
);
GO

CREATE TABLE dbo.Results
(
    ResultId INT IDENTITY(1,1) NOT NULL,
    EnrolmentId INT NOT NULL,
    FinishTime TIME(0) NULL,
    OverallPosition INT NULL,
    CategoryPosition INT NULL,
    AveragePace DECIMAL(6,2) NULL,
    FinishStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Results_FinishStatus DEFAULT N'Finished',

    CONSTRAINT PK_Results PRIMARY KEY (ResultId),
    CONSTRAINT UQ_Results_Enrolment UNIQUE (EnrolmentId),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId) REFERENCES dbo.Enrolments(EnrolmentId),
    CONSTRAINT CK_Results_OverallPosition CHECK (OverallPosition IS NULL OR OverallPosition > 0),
    CONSTRAINT CK_Results_CategoryPosition CHECK (CategoryPosition IS NULL OR CategoryPosition > 0),
    CONSTRAINT CK_Results_Pace CHECK (AveragePace IS NULL OR AveragePace > 0),
    CONSTRAINT CK_Results_FinishStatus CHECK (FinishStatus IN (N'Finished', N'DNF', N'DNS'))
);
GO

/*
    Seed data
    Users: 2 Organisers + 2 Participants
    Events: 3
    Categories: at least 2 per event
    Enrolments: multiple sample participant entries
*/

INSERT INTO dbo.Users (FirstName, LastName, Email, PasswordHash, Role, Phone)
VALUES
(N'Thabo', N'Mokoena', N'thabo@raceday.co.za', N'DEMO_HASH_ORGANISER_001', N'Organiser', N'0825551001'),
(N'Ayanda', N'Naidoo', N'ayanda@raceday.co.za', N'DEMO_HASH_ORGANISER_002', N'Organiser', N'0835551002'),
(N'Lebo', N'Dlamini', N'lebo@example.com', N'DEMO_HASH_PARTICIPANT_001', N'Participant', N'0815552001'),
(N'Jason', N'Mokoena', N'jason@example.com', N'DEMO_HASH_PARTICIPANT_002', N'Participant', N'0845552002');
GO

INSERT INTO dbo.Events
    (OrganiserId, Name, Description, EventDate, RegistrationOpen, RegistrationClose, Venue, City, Province, DistanceKm, EventType, Status)
VALUES
(1, N'Johannesburg City Road Challenge', N'Road running event through central Johannesburg.', '2026-10-11', '2026-08-01', '2026-10-05', N'Constitution Hill', N'Johannesburg', N'Gauteng', 21.10, N'Running', N'Open'),
(2, N'Cape Town Community Cycle', N'Community cycling event with a scenic Cape Town route.', '2026-11-08', '2026-08-15', '2026-11-02', N'Green Point Urban Park', N'Cape Town', N'Western Cape', 42.00, N'Cycling', N'Open'),
(1, N'Durban Family Walk', N'Family-friendly charity walk along the Durban beachfront.', '2026-11-22', '2026-09-01', '2026-11-16', N'Moses Mabhida Stadium', N'Durban', N'KwaZulu-Natal', 10.00, N'Walking', N'Open');
GO

INSERT INTO dbo.Routes
    (EventId, StartLocation, FinishLocation, RouteUrl, ElevationGainM, Notes)
VALUES
(1, N'Constitution Hill', N'Constitution Hill', N'https://example.com/routes/jhb-city-road', 210, N'Urban road loop.'),
(2, N'Green Point Urban Park', N'Green Point Urban Park', N'https://example.com/routes/cape-town-cycle', 480, N'Coastal and city sections.'),
(3, N'Moses Mabhida Stadium', N'Moses Mabhida Stadium', N'https://example.com/routes/durban-family-walk', 65, N'Flat beachfront family route.');
GO

INSERT INTO dbo.Categories
    (EventId, Name, DistanceKm, EntryFee, MaxEntries, MinAge, MaxAge)
VALUES
(1, N'Half Marathon Open', 21.10, 220.00, 3000, 18, NULL),
(1, N'10 km Challenge', 10.00, 120.00, 2500, 16, NULL),
(2, N'42 km Cycle', 42.00, 280.00, 1500, 18, NULL),
(2, N'20 km Social Ride', 20.00, 160.00, 1200, 16, NULL),
(3, N'10 km Family Walk', 10.00, 80.00, 2000, 12, NULL),
(3, N'5 km Fun Walk', 5.00, 50.00, 2500, 8, NULL);
GO

INSERT INTO dbo.Enrolments
    (ParticipantId, EventId, CategoryId, Status)
VALUES
(3, 1, 1, N'Confirmed'),
(4, 1, 2, N'Confirmed'),
(3, 2, 4, N'Confirmed'),
(4, 3, 5, N'Confirmed');
GO

INSERT INTO dbo.Results
    (EnrolmentId, FinishTime, OverallPosition, CategoryPosition, AveragePace, FinishStatus)
VALUES
(1, '01:42:18', 32, 18, 4.85, N'Finished'),
(2, '00:54:37', 41, 21, 5.46, N'Finished');
GO

/* Validation queries for SSMS */
SELECT * FROM dbo.Users;
SELECT * FROM dbo.Events;
SELECT * FROM dbo.Routes;
SELECT * FROM dbo.Categories;
SELECT * FROM dbo.Enrolments;
SELECT * FROM dbo.Results;
GO
