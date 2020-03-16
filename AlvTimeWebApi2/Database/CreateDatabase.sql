CREATE DATABASE AlvDevDB
GO

USE AlvDevDB
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HourRate](
	[FromDate] [datetime2](7) NOT NULL,
	[Rate] [decimal](10, 2) NOT NULL,
	[TaskId] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customer]  ******/
CREATE TABLE [dbo].[Customer](
	[Id] [int] NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[Name] [nvarchar](100) NOT NULL
 )
GO
/****** Object:  Table [dbo].[Hours]  ******/
CREATE TABLE [dbo].[Hours](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL ,
	[User] [int] NOT NULL,
	[Value] [decimal](6, 2) NOT NULL,
	[Date] [datetime] NOT NULL,
	[DayNumber] [smallint] NOT NULL,
	[Year] [smallint] NOT NULL,
	[TaskId] [int] NOT NULL,
	[Locked] [bit] NOT NULL DEFAULT 0
 )
GO
/****** Object:  Table [dbo].[Project]  ******/
CREATE TABLE [dbo].[Project](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Customer] [int] NULL
 )
GO 
/****** Object:  Table [dbo].[Task]    ******/
CREATE TABLE [dbo].[Task](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](300) NOT NULL,
	[Project] [int] NOT NULL,
	[Locked] [bit] NOT NULL DEFAULT 0,
	[Favorite] [bit] NOT NULL DEFAULT 0
)
GO
/****** Object:  Table [dbo].[TaskFavorites]    ******/
CREATE TABLE [dbo].[TaskFavorites](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[UserId] [int] NOT NULL,
	[TaskId] [int] NOT NULL,
)
GO
/****** Object:  Table [dbo].[User] ******/
CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Name] [nvarchar](100) NULL,
	[Email] [nvarchar](100) NULL,
)
GO 
/****** Object:  Table [dbo].[Comment] ******/
CREATE TABLE [dbo].[Comment](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[CommentText] [nvarchar](100) NOT NULL,
)
GO 
ALTER TABLE [dbo].[Task]  WITH CHECK ADD  CONSTRAINT [FK_Task_Project] FOREIGN KEY([Project])
REFERENCES [dbo].[Project] ([Id])
GO
ALTER TABLE [dbo].[Project]  WITH CHECK ADD  CONSTRAINT [FK_Project_Customer] FOREIGN KEY([Customer])
REFERENCES [dbo].[Customer] ([Id])
GO
CREATE VIEW V_DataDump
AS
select
Task.id as taskID,
Task.name as taskName,
Task.project as projectID,
"hours".value,
"hours"."user" as userID,
"hours"."date",
"user".name as userName,
"user".email,
project.name as projectName,
customer.name as customerName,
customer.id as customerId,
HourR.rate as HourRate
from Task
inner join "hours" on taskid = task.id
inner join "user" on "user".id = "hours"."user"
inner join project on project.id = task.project
inner join customer on customer.id = project.Customer
inner join (select HourRate.taskid, HourRate.rate, "hours".id as hourId
				from HourRate
				inner join "hours" on "hours".taskid = HourRate.taskid and "hours".Date >= HourRate.FromDate
				inner join (
					select HourRate.taskId, max(FromDate) as FromDate, "hours".id as hourId
					from HourRate
					inner join "hours" on "hours".taskid = HourRate.taskid and "hours".Date >= HourRate.FromDate
					group by HourRate.taskId, "hours".id
				) maxHours on maxHours.taskid = HourRate.taskid and maxHours.FromDate = HourRate.FromDate and "hours".id = maxHours.hourId
) HourR on HourR.taskid = task.id and "hours".id = HourR.hourId
GO