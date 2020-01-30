CREATE DATABASE AlvDevDB
GO

USE AlvDevDB
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
	[HourRate] [decimal](7, 2) NULL,
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