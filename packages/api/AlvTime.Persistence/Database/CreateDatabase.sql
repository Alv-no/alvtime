CREATE DATABASE AlvDevDB
GO

USE AlvDevDB
GO
/****** Object:  Table [dbo].[HourRate]    Script Date: 29/08/2020 11:50:17 ******/
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
/****** Object:  Table [dbo].[Task]    Script Date: 29/08/2020 11:50:17 ******/
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
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customer]    Script Date: 29/08/2020 11:50:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[InvoiceAddress] [nvarchar](255) NOT NULL,
	[ContactPerson] [nvarchar](100) NOT NULL,
	[ContactEmail] [nvarchar](100) NOT NULL,
	[ContactPhone] [nvarchar](12) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Project]    Script Date: 29/08/2020 11:50:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Project](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Customer] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 29/08/2020 11:50:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[email] [nvarchar](100) NOT NULL,
	[StartDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[hours]    Script Date: 29/08/2020 11:50:17 ******/
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
)WITH (STATISTICS_NORECOMPUTE = ON, IGNORE_DUP_KEY = OFF, FILLFACTOR = 80) ON [PRIMARY],
 CONSTRAINT [UC_hours_user_task] UNIQUE NONCLUSTERED 
(
	[Date] ASC,
	[TaskId] ASC,
	[User] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[V_DataDump]    Script Date: 29/08/2020 11:50:17 ******/
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
/****** Object:  Table [dbo].[__RefactorLog]    Script Date: 29/08/2020 11:50:17 ******/
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
/****** Object:  Table [dbo].[AccessTokens]    Script Date: 29/08/2020 11:50:17 ******/
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
/****** Object:  Table [dbo].[AssociatedTasks]    Script Date: 29/08/2020 11:50:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AssociatedTasks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[TaskId] [int] NOT NULL,
	[FromDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaidOvertime]    Script Date: 29/08/2020 11:50:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaidOvertime](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime2](7) NOT NULL,
	[User] [int] NOT NULL,
	[HoursBeforeCompRate] [decimal](6, 2) NOT NULL,
	[HoursAfterCompRate] [decimal](6, 2) NOT NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[sysdiagrams]    Script Date: 29/08/2020 11:50:17 ******/
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
/****** Object:  Table [dbo].[TaskFavorites]    Script Date: 29/08/2020 11:50:17 ******/
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
CREATE TABLE [dbo].[CompensationRate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FromDate] [dateTime2](7) NOT NULL,
	[Value] [decimal] (4,2) NOT NULL,
	[TaskId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AssociatedTasks] ADD  DEFAULT ('') FOR [EndDate]
GO
ALTER TABLE [dbo].[Customer] ADD  DEFAULT ('') FOR [InvoiceAddress]
GO
ALTER TABLE [dbo].[Customer] ADD  DEFAULT ('') FOR [ContactPerson]
GO
ALTER TABLE [dbo].[Customer] ADD  DEFAULT ('') FOR [ContactEmail]
GO
ALTER TABLE [dbo].[Customer] ADD  DEFAULT ('') FOR [ContactPhone]
GO
ALTER TABLE [dbo].[hours] ADD  DEFAULT ((0)) FOR [Locked]
GO
ALTER TABLE [dbo].[Task] ADD  DEFAULT ((0)) FOR [Locked]
GO
ALTER TABLE [dbo].[Task] ADD  DEFAULT ((0)) FOR [Favorite]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [StartDate]
GO
ALTER TABLE [dbo].[AccessTokens]  WITH CHECK ADD  CONSTRAINT [FK_AccessTokens_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[CompensationRate]  WITH CHECK ADD  CONSTRAINT [FK_CompensationRate_Task] FOREIGN KEY([TaskId])
REFERENCES [dbo].[Task] ([Id])
GO
ALTER TABLE [dbo].[CompensationRate] CHECK CONSTRAINT [FK_CompensationRate_Task]
GO
ALTER TABLE [dbo].[AccessTokens] CHECK CONSTRAINT [FK_AccessTokens_User]
GO
ALTER TABLE [dbo].[AssociatedTasks]  WITH CHECK ADD  CONSTRAINT [FK_AssociatedTasks_Task] FOREIGN KEY([TaskId])
REFERENCES [dbo].[Task] ([Id])
GO
ALTER TABLE [dbo].[AssociatedTasks] CHECK CONSTRAINT [FK_AssociatedTasks_Task]
GO
ALTER TABLE [dbo].[AssociatedTasks]  WITH CHECK ADD  CONSTRAINT [FK_AssociatedTasks_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[AssociatedTasks] CHECK CONSTRAINT [FK_AssociatedTasks_User]
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
ALTER TABLE [dbo].[PaidOvertime]  WITH CHECK ADD  CONSTRAINT [FK_PaidOvertime_User] FOREIGN KEY([User])
REFERENCES [dbo].[User] ([Id])
GO
ALTER TABLE [dbo].[PaidOvertime] CHECK CONSTRAINT [FK_PaidOvertime_User]
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
EXEC sys.sp_addextendedproperty @name=N'microsoft_database_tools_support', @value=N'refactoring log' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'__RefactorLog'
GO
EXEC sys.sp_addextendedproperty @name=N'microsoft_database_tools_support', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'sysdiagrams'
GO
