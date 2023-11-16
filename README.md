# SSHUserManagement
این بات برای مدیریت یوزر های سرور SSH نوشته شده :
در قدم اول ربات خود را تولید کنید همچنین دستورات زیر را هم در ربات تلگرام تعریف کنید 
```Bot-Command-Config
create - create ssh user
delete -delete userid
userinfo - Get User info
```

در قدم بعدی فایل appsettings.json را شخصی سازی کنید و پروژه را به ربات خود متصل کنید (در نظر داشته باشید روی سرور میبایست root باشید)

در قدم بعدی این اسکریپت هم روی SQLServer اجرا کنید 
```SQL
 SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[Id] [BIGINT] IDENTITY(1,1) NOT NULL,
	[Name] [NVARCHAR](500) NULL,
	[Email] [NVARCHAR](500) NULL,
	[Password] [NVARCHAR](500) NULL,
	[ChatId] [BIGINT] NULL,
	[ExpDate] [NVARCHAR](500) NULL,
	[TelegramId] [NVARCHAR](500) NULL,
	[IsDeleted] [BIT] NULL
) ON [PRIMARY]
GO
```



تمام !
