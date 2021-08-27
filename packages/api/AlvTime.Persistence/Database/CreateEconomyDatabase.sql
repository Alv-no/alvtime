
/****** Object:  Database [AlvEconomyData]    Script Date: 8/27/2021 9:25:02 AM ******/
CREATE DATABASE AlvEconomyData
GO

USE AlvEconomyData
GO
/****** Object:  Table [dbo].[EmployeeHourlySalaries]    Script Date: 8/27/2021 9:25:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeHourlySalaries](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[HourlySalary] [decimal](18, 9) NOT NULL,
	[FromDateInclusive] [datetime2](7) NOT NULL,
	[ToDate] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OvertimePayout]    Script Date: 8/27/2021 9:25:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OvertimePayout](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Date] [datetime2](7) NOT NULL,
	[TotalPayout] [decimal](18, 9) NOT NULL,
	[RegisteredPaidOvertimeId] [int] NOT NULL,
 CONSTRAINT [PK_OvertimePayout] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
USE [master]
GO
ALTER DATABASE [AlvEconomyData] SET  READ_WRITE 
GO
