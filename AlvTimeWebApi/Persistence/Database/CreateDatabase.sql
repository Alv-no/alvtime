CREATE DATABASE AlvDevDB
GO

USE AlvDevDB
GO

/****** Object:  Table [dbo].[HourRate]    Script Date: 24/03/2020 14:21:08 ******/
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
/****** Object:  Table [dbo].[Customer]    Script Date: 24/03/2020 14:21:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[Id] [int] IDENTITY NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[InvoiceAddress] [nvarchar](255) NOT NULL,
	[ContactPerson] [nvarchar](100) NOT NULL,
	[ContactEmail] [nvarchar](100) NOT NULL,
	[ContactPhone] [nvarchar](12) NOT NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Project]    Script Date: 24/03/2020 14:21:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Project](
	[Id] [int] IDENTITY NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Customer] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Task]    Script Date: 24/03/2020 14:21:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Task](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](300) NOT NULL,
	[Project] [int] NOT NULL,
	[Locked] [bit] NOT NULL,
	[Favorite] [bit] NOT NULL,
	[CompensationRate] [decimal](3,2) NOT NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 24/03/2020 14:21:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY  NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[email] [nvarchar](100) NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[FlexiHours] [decimal](5,2) NOT NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[hours]    Script Date: 24/03/2020 14:21:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[hours](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[User] [int] NOT NULL,
	[Value] [decimal](6, 2) NOT NULL,
	[Date] [datetime] NOT NULL,
	[DayNumber] [smallint] NOT NULL,
	[Year] [smallint] NOT NULL,
	[TaskId] [int] NOT NULL,
	[Locked] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UC_hours_user_task] UNIQUE NONCLUSTERED 
(
	[Date] ASC,
	[TaskId] ASC,
	[User] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[V_DataDump]    Script Date: 24/03/2020 14:21:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[V_DataDump]
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
HourR.rate as HourRate,
HourR.rate * "hours".value as earnings,
CAST(CASE WHEN HourR.rate > 0 THEN 1 ELSE 0 END AS BIT) as IsBillable
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
/****** Object:  Table [dbo].[__RefactorLog]    Script Date: 24/03/2020 14:21:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__RefactorLog](
	[OperationKey] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[OperationKey] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[sysdiagrams]    Script Date: 24/03/2020 14:21:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[sysdiagrams](
	[name] [sysname] NOT NULL,
	[principal_id] [int] NOT NULL,
	[diagram_id] [int] IDENTITY(1,1) NOT NULL,
	[version] [int] NULL,
	[definition] [varbinary](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[diagram_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_principal_name] UNIQUE NONCLUSTERED 
(
	[principal_id] ASC,
	[name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TaskFavorites]    Script Date: 24/03/2020 14:21:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaskFavorites](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[TaskId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccessTokens](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[FriendlyName] [nvarchar](100) NOT NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[Value] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[hours] ADD  DEFAULT ((0)) FOR [Locked]
GO
ALTER TABLE [dbo].[Task] ADD  DEFAULT ((0)) FOR [Locked]
GO
ALTER TABLE [dbo].[Task] ADD  DEFAULT ((0)) FOR [Favorite]
GO
ALTER TABLE [dbo].[HourRate]  WITH CHECK ADD  CONSTRAINT [FK_HourRate_Task] FOREIGN KEY([TaskId])
REFERENCES [dbo].[Task] ([Id])
GO
ALTER TABLE [dbo].[HourRate] CHECK CONSTRAINT [FK_HourRate_Task]
GO
ALTER TABLE [dbo].[hours]  WITH CHECK ADD  CONSTRAINT [FK_hours_Task] FOREIGN KEY([TaskId])
REFERENCES [dbo].[Task] ([Id])
GO
ALTER TABLE [dbo].[hours] CHECK CONSTRAINT [FK_hours_Task]
GO
ALTER TABLE [dbo].[hours]  WITH CHECK ADD  CONSTRAINT [FK_hours_User] FOREIGN KEY([User])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[hours] CHECK CONSTRAINT [FK_hours_User]
GO
ALTER TABLE [dbo].[Project]  WITH CHECK ADD  CONSTRAINT [FK_Project_Customer] FOREIGN KEY([Customer])
REFERENCES [dbo].[Customer] ([Id])
GO
ALTER TABLE [dbo].[Project] CHECK CONSTRAINT [FK_Project_Customer]
GO
ALTER TABLE [dbo].[Task]  WITH CHECK ADD  CONSTRAINT [FK_Task_Project] FOREIGN KEY([Project])
REFERENCES [dbo].[Project] ([Id])
GO
ALTER TABLE [dbo].[Task] CHECK CONSTRAINT [FK_Task_Project]
GO
ALTER TABLE [dbo].[TaskFavorites]  WITH CHECK ADD  CONSTRAINT [FK_TaskFavorites_Task] FOREIGN KEY([TaskId])
REFERENCES [dbo].[Task] ([Id])
GO
ALTER TABLE [dbo].[TaskFavorites] CHECK CONSTRAINT [FK_TaskFavorites_Task]
GO
ALTER TABLE [dbo].[TaskFavorites]  WITH CHECK ADD  CONSTRAINT [FK_TaskFavorites_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[TaskFavorites] CHECK CONSTRAINT [FK_TaskFavorites_User]
GO
ALTER TABLE [dbo].[AccessTokens]  WITH CHECK ADD  CONSTRAINT [FK_AccessTokens_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO