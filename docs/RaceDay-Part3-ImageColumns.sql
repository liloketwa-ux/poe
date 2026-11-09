USE RaceDayDB;
GO
IF COL_LENGTH('dbo.Events', 'BannerImageUrl') IS NULL
    ALTER TABLE dbo.Events ADD BannerImageUrl NVARCHAR(1000) NULL;
GO
IF COL_LENGTH('dbo.Users', 'ProfileImageUrl') IS NULL
    ALTER TABLE dbo.Users ADD ProfileImageUrl NVARCHAR(1000) NULL;
GO
