/****** Object:  User [myacc]    Script Date: 1/13/2020 11:45:25 PM ******/
CREATE USER [myacc] FOR LOGIN [myacc] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [myacc]
GO
ALTER ROLE [db_accessadmin] ADD MEMBER [myacc]
GO
ALTER ROLE [db_securityadmin] ADD MEMBER [myacc]
GO
ALTER ROLE [db_ddladmin] ADD MEMBER [myacc]
GO
ALTER ROLE [db_backupoperator] ADD MEMBER [myacc]
GO
ALTER ROLE [db_datareader] ADD MEMBER [myacc]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [myacc]
GO
ALTER ROLE [db_denydatareader] ADD MEMBER [myacc]
GO
ALTER ROLE [db_denydatawriter] ADD MEMBER [myacc]
GO
/****** Object:  UserDefinedFunction [dbo].[buildCondition]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[buildCondition]
(
	@op nvarchar(1),
	@criteria nvarchar(255)
)
RETURNS nvarchar(300)
AS
BEGIN
IF @op = '0' goto return_equal
IF @op = '1' goto return_notequal
IF @op = '2' goto return_contain
IF @op = '3' goto return_notcontain

return_equal:
RETURN ' = ' + char(39) + @criteria + char(39);

return_notequal:
RETURN ' <> ' + char(39) + @criteria + char(39);

return_contain:
RETURN ' LIKE ' + char(39) + '%' + @criteria + '%' + char(39);

return_notcontain:
RETURN ' NOT LIKE ' + char(39) + '%' + @criteria + '%' + char(39);
END
GO
/****** Object:  UserDefinedFunction [dbo].[calculateWorkHour]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[calculateWorkHour]
(
	@timeStart nvarchar(max),
	@timeEnd nvarchar(max)
)
RETURNS BIGINT
AS
BEGIN
--start
Declare @checkIn nvarchar(max), @checkOut nvarchar(max), @breakStart nvarchar(max), @breakEnd nvarchar(max);

SET @checkIn = (SELECT fValue from commonParam WHERE param = 'checkIn');
SET @checkOut = (SELECT fValue from commonParam WHERE param = 'checkOut');
SET @breakStart = (SELECT fValue from commonParam WHERE param = 'breakStart');
SET @breakEnd = (SELECT fValue from commonParam WHERE param = 'breakEnd');

declare @dateStartString nvarchar(max), @dateEndString nvarchar(max);
declare @clockStartString nvarchar(max), @clockEndString nvarchar(max);
SET @dateStartString = LEFT(@timeStart,10);
SET @dateEndString = LEFT(@timeEnd,10);
SET @clockStartString = RIGHT(@timeStart,5);
SET @clockEndString = RIGHT(@timeEnd,5);

declare @dateStart DATE, @dateEnd DATE;
select @dateStart = cast(@dateStartString as date), @dateEnd = cast(@dateEndString as date)

IF @dateStart > @dateEnd RETURN 0;



--
declare @checkInAbs BIGINT = cast(left(@checkIn,2) as int)*60 + cast(right(@checkIn,2) as int)
declare @breakStartAbs BIGINT = cast(left(@breakStart,2) as int)*60 + cast(right(@breakStart,2) as int)
declare @breakEndAbs BIGINT = cast(left(@breakEnd,2) as int)*60 + cast(right(@breakEnd,2) as int)
declare @checkOutAbs BIGINT = cast(left(@checkOut,2) as int)*60 + cast(right(@checkOut,2) as int)
declare @clockStartAbs BIGINT = cast(left(@clockStartString,2) as int)*60 + cast(right(@clockStartString,2) as int)
declare @clockEndAbs BIGINT = cast(left(@clockEndString,2) as int)*60 + cast(right(@clockEndString,2) as int)

declare @normalWorkingLength BIGINT = @checkOutAbs - @breakEndAbs + @breakStartAbs - @checkInAbs

IF @clockStartAbs > @checkOutAbs SET @clockStartAbs = @checkOutAbs;
IF @clockEndAbs > @checkOutAbs SET @clockEndAbs = @checkOutAbs;
IF @clockStartAbs < @checkInAbs SET @clockStartAbs = @checkInAbs;
IF @clockEndAbs < @checkInAbs SET @clockEndAbs = @checkInAbs;


IF @clockStartAbs > @breakStartAbs AND @clockStartAbs < @breakEndAbs SET @clockStartAbs = @breakEndAbs;
IF @clockEndAbs > @breakStartAbs AND @clockEndAbs < @breakEndAbs SET @clockEndAbs = @breakStartAbs;

if @dateStart = @dateEnd
	BEGIN
		IF EXISTS (SELECT * FROM weeklyDayOff t2 WHERE t2.weekDay = DATENAME(weekday,@dateStart)) RETURN 0;
		IF EXISTS (SELECT * FROM publicHoliday t2 WHERE t2.holiday = CONVERT(nvarchar(20),@dateStart,111)) RETURN 0;
		IF @clockStartAbs >= @clockEndAbs RETURN 0
		IF @clockStartAbs <= @breakStartAbs AND @clockEndAbs <= @breakStartAbs RETURN ceiling(cast(@clockEndAbs - @clockStartAbs as decimal)/cast(60 as decimal));
		IF @clockStartAbs >= @breakEndAbs AND @clockEndAbs >= @breakEndAbs RETURN ceiling(cast(@clockEndAbs - @clockStartAbs as decimal)/cast(60 as decimal));
		IF @clockStartAbs <= @breakStartAbs AND @clockEndAbs >= @breakEndAbs 
			RETURN ceiling(cast(@clockEndAbs - @breakEndAbs + @breakStartAbs - @clockStartAbs as decimal)/cast(60 as decimal));
	END
--
declare @dateStartPlus1 DATE = dateadd(day,1,@dateStart), @dateEndMinus1 DATE = dateadd(day,-1,@dateEnd);
DECLARE @publicHoliday TABLE
(
  holiday nvarchar(20),
  wDay nvarchar(10)
);
INSERT INTO @publicHoliday (holiday, wDay) SELECT t1.holiday AS holiday, DATENAME(weekday,t1.holiday) AS wDay
	FROM publicHoliday t1
	WHERE t1.holiday > @dateStartString AND t1.holiday < @dateEndString
	AND len(t1.holiday) = 10
	AND NOT EXISTS (SELECT * FROM weeklyDayOff t2 WHERE t2.weekDay = DATENAME(weekday,t1.holiday));

DECLARE @normalWorkingDay bigint = 0;
--
WHILE @dateStartPlus1 <= @dateEndMinus1
	BEGIN
		IF NOT EXISTS (SELECT * FROM weeklyDayOff t2 WHERE t2.weekDay = DATENAME(weekday,@dateStartPlus1))
			BEGIN
				IF NOT EXISTS (SELECT * FROM @publicHoliday t2 WHERE t2.holiday = CONVERT(nvarchar(20),@dateStartPlus1,111))
					BEGIN
						SET @normalWorkingDay = @normalWorkingDay + 1					
					END
			END
		SET @dateStartPlus1 = dateadd(day,1,@dateStartPlus1)
	END;
--

--end
DECLARE @startDayWorkingLength BIGINT, @endDayWorkingLength BIGINT

IF @clockStartAbs <= @breakStartAbs SET @startDayWorkingLength = @checkOutAbs - @breakEndAbs + @breakStartAbs - @clockStartAbs;
IF @clockStartAbs >= @breakEndAbs SET @startDayWorkingLength = @checkOutAbs - @clockStartAbs;
IF @clockEndAbs <= @breakStartAbs SET @endDayWorkingLength = @clockEndAbs - @checkInAbs;
IF @clockEndAbs >= @breakEndAbs SET @endDayWorkingLength = @clockEndAbs - @breakEndAbs + @breakStartAbs - @checkInAbs;


IF EXISTS (SELECT * FROM weeklyDayOff t2 WHERE t2.weekDay = DATENAME(weekday,@dateStart)) SET @startDayWorkingLength = 0;
IF EXISTS (SELECT * FROM publicHoliday t2 WHERE t2.holiday = CONVERT(nvarchar(20),@dateStart,111)) SET @startDayWorkingLength = 0;
IF EXISTS (SELECT * FROM weeklyDayOff t2 WHERE t2.weekDay = DATENAME(weekday,@dateEnd)) SET @endDayWorkingLength = 0;
IF EXISTS (SELECT * FROM publicHoliday t2 WHERE t2.holiday = CONVERT(nvarchar(20),@dateEnd,111)) SET @endDayWorkingLength = 0;


RETURN CEILING(CAST(@normalWorkingDay*@normalWorkingLength + @startDayWorkingLength + @endDayWorkingLength AS decimal)/CAST(60 AS decimal));

END
GO
/****** Object:  UserDefinedFunction [dbo].[Function]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Function]
(
	@param1 int,
	@param2 int
)
RETURNS INT
AS
BEGIN
	RETURN @param1 + @param2
END
GO
/****** Object:  UserDefinedFunction [dbo].[HASHPW]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[HASHPW]
(	
	-- Add the parameters for the function here
	@RawPW nvarchar(50),
	@username nvarchar(50)
)
RETURNS nvarchar(65)
AS
BEGIN
	RETURN CONVERT(NVARCHAR(65),HashBytes('SHA2_256', @RawPW + 'LeaveMS' + REVERSE(@username)),1);
END;

GO
/****** Object:  UserDefinedFunction [dbo].[udfInitAppID]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[udfInitAppID]
(
)
RETURNS NVARCHAR(16)
AS
BEGIN
declare @lastSeq bigint;
declare @initYear int;
SET @initYEar = YEAR(GETDATE());
SET @lastSeq = (select count(*) from leaveApplicationData t1 where year(t1.initDate) = @initYear)+1;

RETURN 'LR'+ CONVERT(nvarchar,@initYear) + Right('0000000' + CONVERT(NVARCHAR, @lastSeq), 7);

END
GO
/****** Object:  UserDefinedFunction [dbo].[UdfMaxInt]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[UdfMaxInt]
(
@a bigint
, @b bigint
)
returns bigint
as
begin
return (select case when @a > @b then @a else @b end)
end
GO
/****** Object:  UserDefinedFunction [dbo].[validateApp]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[validateApp]
(
	@appID nvarchar(20)
)
RETURNS BIT
AS
BEGIN
DECLARE @reasonCode nvarchar(20), @correspondingColumn nvarchar(20);
DECLARE @userID nvarchar(max);
DECLARE @leaveQuota nvarchar(20);
DECLARE @appYear nvarchar(20);
DECLARE @quota bigint, @hourUsed1 bigint, @hourUsed2 bigint, @hourRequired bigint;

IF NOT EXISTS (SELECT * FROM leaveApplicationData WHERE appID = @appID) GOTO return_0;

SELECT @userID = userID,
		@reasonCode = reasonCode,
		@appYear = LEFT(timeStart,4),
		@hourRequired = hourRequired
	FROM leaveApplicationData WHERE appID = @appID;

SET @correspondingColumn = 'yearly' + @reasonCode;
SET @leaveQuota = (SELECT 
	(CASE @correspondingColumn
		WHEN 'yearlyANNL' THEN t1.yearlyANNL
		WHEN 'yearlyBRML' THEN t1.yearlyBRML
		WHEN 'yearlyCPSL' THEN t1.yearlyCPSL
		WHEN 'yearlyCPSL' THEN t1.yearlyFMRL	
		WHEN 'yearlyCPSL' THEN t1.yearlySCKL
		WHEN 'yearlyCPSL' THEN t1.yearlySMRL
		WHEN 'yearlyCPSL' THEN t1.yearlySPCL
		WHEN 'yearlyCPSL' THEN t1.yearlyUPDL
	END)
	FROM [mData-LeaveQuota] t1
	WHERE t1.userID = @userID);

IF @leaveQuota = 'Unlimited' GOTO return_1;
SET @quota = CAST(@leaveQuota AS bigint);

SELECT @hourUsed1 = SUM(hourRequired) FROM leaveApplicationData
	WHERE userID = @userID
	AND reasonCode = @reasonCode
	AND applicationProgress = 'submitted'
	AND systemStatus = 'normal'
	AND LEFT(timeStart,4) = @appYear
	AND appID <> @appID;

IF @hourUsed1 IS NULL SET @hourUsed1 = 0;
SELECT @hourUsed2 = SUM(hourRequired) FROM leaveApplicationData
	WHERE userID = @userID
	AND reasonCode = @reasonCode
	AND applicationProgress = 'Done'
	AND approverAction = 'Approve'
	AND systemStatus = 'normal'
	AND LEFT(timeStart,4) = @appYear
	AND appID <> @appID;
IF @hourUsed2 IS NULL SET @hourUsed2 = 0;
IF @quota - @hourUsed1 - @hourUsed2 < @hourRequired GOTO return_0


return_1:
RETURN 1

return_0:
RETURN 0

END
GO
/****** Object:  Table [dbo].[commonParam]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[commonParam](
	[param] [nvarchar](50) NOT NULL,
	[fValue] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[param] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[deptList]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[deptList](
	[deptCode] [nvarchar](50) NOT NULL,
	[deptName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_deptList] PRIMARY KEY CLUSTERED 
(
	[deptCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[leaveApplicationData]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[leaveApplicationData](
	[appID] [nvarchar](50) NOT NULL,
	[initDate] [date] NULL,
	[userID] [nvarchar](50) NULL,
	[deptCode] [nvarchar](50) NULL,
	[rankCode] [nvarchar](50) NULL,
	[reasonCode] [nvarchar](50) NULL,
	[applicantDesc] [nvarchar](200) NULL,
	[timeStart] [nvarchar](50) NULL,
	[timeEnd] [nvarchar](50) NULL,
	[hourRequired] [int] NULL,
	[validationStatus] [nvarchar](50) NULL,
	[approverUserID] [nvarchar](50) NULL,
	[approverAction] [nvarchar](50) NULL,
	[approverDesc] [nvarchar](200) NULL,
	[applicationProgress] [nvarchar](50) NULL,
	[systemStatus] [nvarchar](50) NULL,
	[recordChangeLog] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[appID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[mData-LeaveQuota]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mData-LeaveQuota](
	[userID] [nvarchar](50) NOT NULL,
	[yearlyANNL] [nvarchar](50) NULL DEFAULT ((96)),
	[yearlyBRML] [nvarchar](50) NULL DEFAULT ((24)),
	[yearlyCPSL] [nvarchar](50) NULL DEFAULT ('Unlimited'),
	[yearlyFMRL] [nvarchar](50) NULL DEFAULT ((16)),
	[yearlySMRL] [nvarchar](50) NULL DEFAULT ((32)),
	[yearlySCKL] [nvarchar](50) NULL DEFAULT ((240)),
	[yearlyUPDL] [nvarchar](50) NULL DEFAULT ((480)),
	[yearlySPCL] [nvarchar](50) NULL DEFAULT ('Unlimited'),
PRIMARY KEY CLUSTERED 
(
	[userID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[mData-LeaveReason]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[mData-LeaveReason](
	[reasonCode] [nvarchar](50) NOT NULL,
	[reasonDetail] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[reasonCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[publicHoliday]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[publicHoliday](
	[holiday] [nvarchar](50) NOT NULL,
	[description] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[holiday] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[rankList]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[rankList](
	[rankCode] [nvarchar](50) NOT NULL,
	[rankDescription] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_rankList] PRIMARY KEY CLUSTERED 
(
	[rankCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[userList]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[userList](
	[userID] [nvarchar](50) NOT NULL,
	[userName] [nvarchar](50) NOT NULL,
	[userPassword] [nvarchar](65) NULL,
	[deptCode] [nvarchar](50) NULL,
	[rankCode] [nvarchar](50) NULL,
	[userEmail] [nvarchar](50) NOT NULL,
	[userIsActive] [bit] NULL DEFAULT ((1)),
	[userFailedLoginAttempt] [int] NULL DEFAULT ((0)),
 CONSTRAINT [PK_userList] PRIMARY KEY CLUSTERED 
(
	[userID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[weeklyDayOff]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[weeklyDayOff](
	[weekDay] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_weeklyDayOff] PRIMARY KEY CLUSTERED 
(
	[weekDay] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
INSERT [dbo].[commonParam] ([param], [fValue]) VALUES (N'breakEnd', N'13:30')
INSERT [dbo].[commonParam] ([param], [fValue]) VALUES (N'breakStart', N'12:30')
INSERT [dbo].[commonParam] ([param], [fValue]) VALUES (N'checkIn', N'08:30')
INSERT [dbo].[commonParam] ([param], [fValue]) VALUES (N'checkOut', N'17:30')
INSERT [dbo].[deptList] ([deptCode], [deptName]) VALUES (N'0000', N'BOD')
INSERT [dbo].[deptList] ([deptCode], [deptName]) VALUES (N'0001', N'IT')
INSERT [dbo].[deptList] ([deptCode], [deptName]) VALUES (N'0002', N'HR-GA')
INSERT [dbo].[deptList] ([deptCode], [deptName]) VALUES (N'0003', N'Supply Chain and Logistics')
INSERT [dbo].[deptList] ([deptCode], [deptName]) VALUES (N'0004', N'Production')
INSERT [dbo].[deptList] ([deptCode], [deptName]) VALUES (N'0005', N'Finance')
INSERT [dbo].[deptList] ([deptCode], [deptName]) VALUES (N'0006', N'Sale and Distribution')
INSERT [dbo].[deptList] ([deptCode], [deptName]) VALUES (N'0007', N'Facility')
INSERT [dbo].[leaveApplicationData] ([appID], [initDate], [userID], [deptCode], [rankCode], [reasonCode], [applicantDesc], [timeStart], [timeEnd], [hourRequired], [validationStatus], [approverUserID], [approverAction], [approverDesc], [applicationProgress], [systemStatus], [recordChangeLog]) VALUES (N'LR20200000001', CAST(N'2020-01-01' AS Date), N'manager1', N'0002', N'Manager', N'ANNL', N'nghỉ không phép, admin cấn trừ phép', N'2020/01/08 21:55', N'2020/01/09 21:55', 8, N'validated', N'sysadmin', N'Approve', NULL, N'Done', N'disabled', NULL)
INSERT [dbo].[leaveApplicationData] ([appID], [initDate], [userID], [deptCode], [rankCode], [reasonCode], [applicantDesc], [timeStart], [timeEnd], [hourRequired], [validationStatus], [approverUserID], [approverAction], [approverDesc], [applicationProgress], [systemStatus], [recordChangeLog]) VALUES (N'LR20200000002', CAST(N'2020-01-03' AS Date), N'user1', N'0002', N'User', N'ANNL', N'dmm', N'2020/01/14 00:22', N'2020/01/15 00:22', 8, N'validated', NULL, NULL, NULL, N'submitted', N'normal', NULL)
INSERT [dbo].[leaveApplicationData] ([appID], [initDate], [userID], [deptCode], [rankCode], [reasonCode], [applicantDesc], [timeStart], [timeEnd], [hourRequired], [validationStatus], [approverUserID], [approverAction], [approverDesc], [applicationProgress], [systemStatus], [recordChangeLog]) VALUES (N'LR20200000003', CAST(N'2020-01-03' AS Date), N'user1', N'0002', N'User', N'ANNL', N'dmm', N'2020/01/14 00:22', N'2020/01/15 00:22', 8, N'validated', NULL, NULL, NULL, N'submitted', N'normal', NULL)
INSERT [dbo].[leaveApplicationData] ([appID], [initDate], [userID], [deptCode], [rankCode], [reasonCode], [applicantDesc], [timeStart], [timeEnd], [hourRequired], [validationStatus], [approverUserID], [approverAction], [approverDesc], [applicationProgress], [systemStatus], [recordChangeLog]) VALUES (N'LR20200000004', CAST(N'2020-01-03' AS Date), N'user1', N'0002', N'User', N'ANNL', N'dmm', N'2020/01/14 00:25', N'2020/01/15 00:26', 8, N'validated', NULL, NULL, NULL, N'submitted', N'normal', NULL)
INSERT [dbo].[leaveApplicationData] ([appID], [initDate], [userID], [deptCode], [rankCode], [reasonCode], [applicantDesc], [timeStart], [timeEnd], [hourRequired], [validationStatus], [approverUserID], [approverAction], [approverDesc], [applicationProgress], [systemStatus], [recordChangeLog]) VALUES (N'LR20200000005', CAST(N'2020-01-03' AS Date), N'user1', N'0002', N'User', N'ANNL', N'xxxx', N'2020/01/14 00:29', N'2020/01/15 00:29', 8, N'validated', NULL, NULL, NULL, N'submitted', N'normal', NULL)
INSERT [dbo].[leaveApplicationData] ([appID], [initDate], [userID], [deptCode], [rankCode], [reasonCode], [applicantDesc], [timeStart], [timeEnd], [hourRequired], [validationStatus], [approverUserID], [approverAction], [approverDesc], [applicationProgress], [systemStatus], [recordChangeLog]) VALUES (N'LR20200000006', CAST(N'2020-01-03' AS Date), N'user1', N'0002', N'User', N'ANNL', N'ffffffffff', N'2020/01/14 00:32', N'2020/01/15 00:32', 8, N'validated', NULL, NULL, NULL, N'submitted', N'normal', NULL)
INSERT [dbo].[leaveApplicationData] ([appID], [initDate], [userID], [deptCode], [rankCode], [reasonCode], [applicantDesc], [timeStart], [timeEnd], [hourRequired], [validationStatus], [approverUserID], [approverAction], [approverDesc], [applicationProgress], [systemStatus], [recordChangeLog]) VALUES (N'LR20200000007', CAST(N'2020-01-03' AS Date), N'user1', N'0002', N'User', N'ANNL', N'thich la nghi', N'2020/01/15 22:26', N'2020/01/23 22:26', 48, N'validated', NULL, NULL, NULL, N'submitted', N'normal', NULL)
INSERT [dbo].[leaveApplicationData] ([appID], [initDate], [userID], [deptCode], [rankCode], [reasonCode], [applicantDesc], [timeStart], [timeEnd], [hourRequired], [validationStatus], [approverUserID], [approverAction], [approverDesc], [applicationProgress], [systemStatus], [recordChangeLog]) VALUES (N'LR20200000008', CAST(N'2020-01-12' AS Date), N'manager1', N'0002', N'Manager', NULL, NULL, NULL, NULL, NULL, N'not validated', NULL, NULL, NULL, N'initialized', N'normal', NULL)
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'ccc1', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'director1', N'Unlimited', N'Unlimited', N'Unlimited', N'Unlimited', N'Unlimited', N'Unlimited', N'Unlimited', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'manager1', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'managerIT', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'managerPROD', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'managerSCM', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'managerSD', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'user1', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'user2', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'userIT1', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'userprod1', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveQuota] ([userID], [yearlyANNL], [yearlyBRML], [yearlyCPSL], [yearlyFMRL], [yearlySMRL], [yearlySCKL], [yearlyUPDL], [yearlySPCL]) VALUES (N'userSCM 1', N'96', N'24', N'Unlimited', N'16', N'32', N'240', N'480', N'Unlimited')
INSERT [dbo].[mData-LeaveReason] ([reasonCode], [reasonDetail]) VALUES (N'ANNL', N' Annual Leave - Nghỉ phép')
INSERT [dbo].[mData-LeaveReason] ([reasonCode], [reasonDetail]) VALUES (N'BRML', N'Bereavement  Leave - Nghỉ tang lễ')
INSERT [dbo].[mData-LeaveReason] ([reasonCode], [reasonDetail]) VALUES (N'CPSL', N'Compensation Leave - Nghỉ bù')
INSERT [dbo].[mData-LeaveReason] ([reasonCode], [reasonDetail]) VALUES (N'FMRL', N'Marriage Leave For  Fam. - Nghỉ cưới cho gia đình')
INSERT [dbo].[mData-LeaveReason] ([reasonCode], [reasonDetail]) VALUES (N'SCKL', N'Sick Leave - Nghỉ ốm')
INSERT [dbo].[mData-LeaveReason] ([reasonCode], [reasonDetail]) VALUES (N'SMRL', N'Marriage Leave For Self - Nghỉ cưới cho bản thân')
INSERT [dbo].[mData-LeaveReason] ([reasonCode], [reasonDetail]) VALUES (N'SPCL', N'Special Leave - Nghỉ đặc biệt')
INSERT [dbo].[mData-LeaveReason] ([reasonCode], [reasonDetail]) VALUES (N'UPDL', N'Unpaid Leave - Nghỉ không lương')
INSERT [dbo].[publicHoliday] ([holiday], [description]) VALUES (N'2019/01/01', N'tet duong lich 2019')
INSERT [dbo].[publicHoliday] ([holiday], [description]) VALUES (N'2019/04/30', N'30/4')
INSERT [dbo].[publicHoliday] ([holiday], [description]) VALUES (N'2019/05/01', N'quoc te lao dong 2019')
INSERT [dbo].[publicHoliday] ([holiday], [description]) VALUES (N'2019/09/02', N'quoc khanh 2019')
INSERT [dbo].[publicHoliday] ([holiday], [description]) VALUES (N'2020/01/01', N'tet tay')
INSERT [dbo].[publicHoliday] ([holiday], [description]) VALUES (N'2020/01/01 disabled at 2020-01-03T21:42:19.377', N'tet tay 2020')
INSERT [dbo].[publicHoliday] ([holiday], [description]) VALUES (N'2020/01/27', N'tet')
INSERT [dbo].[rankList] ([rankCode], [rankDescription]) VALUES (N'Admin', N'System Administrator')
INSERT [dbo].[rankList] ([rankCode], [rankDescription]) VALUES (N'Director', N'Director Level')
INSERT [dbo].[rankList] ([rankCode], [rankDescription]) VALUES (N'Manager', N'Manager Level')
INSERT [dbo].[rankList] ([rankCode], [rankDescription]) VALUES (N'User', N'Normal User')
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'ccc1', N'ccc1', N'0xC208EACF480AB5D6E6AE2CFC46D17D3D1102FDE830919DC61D48C96C6CF761', N'0001', N'User', N'xxx@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'director1', N'Director 1', N'0x191BC1D52ADC99E2C6983FB41BCA9AFD455398CAB538AE7542875EBBBE4B0D', N'0000', N'Director', N'director1@gmail.com', 0, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'manager1', N'Manager 1', N'0x4C36F0BE3FF76A0F56ED1C16A7896053CBC1838F73F949D7C7B7B0D1F4675D', N'0002', N'Manager', N'manager1@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'managerIT', N'Manager IT', N'0xBFA45C65D32F3C34CEB40AF98A8443F9365F912B8FE18EB178E51EF3CA6075', N'0001', N'Manager', N'managerIT@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'managerPROD', N'Manager Production', N'0xABCB36BEBF716ACA5063D7EC8FC356C0464581FAF344D627343AB1C118B0E5', N'0004', N'Manager', N'managerPROD@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'managerSCM', N'Manager SCM', N'0x9D2E06D3DEF2858ABF6CDFE581B361C46DCBFF6814262B6586088A8E4296C0', N'0003', N'Manager', N'managerSCM@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'managerSD', N'Manager SD', N'0x28A6D17690DF6C2A00245DE0A5335FF07E90E3DB9EBAD0347BB943BD9B1B98', N'0006', N'Manager', N'managerSD@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'sysadmin', N'System Admin', N'0x242CA0C93A0AECD4C7FA5C6230B9A67523F76CF0E2A30C389A7759E8553AA3', N'0000', N'Admin', N'sysadminLeaveMS@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'user1', N'User 1', N'0xE34B24B59365CDEFE316CF238ACB05FBF1D5412F94D40B4A4A6E869BE42776', N'0002', N'User', N'user1@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'user2', N'User 2', N'0x8E9B0337E669975748A47B63D6C3ADB28ADA76A79B4D0C17F95AD95DD061DE', N'0005', N'User', N'user2@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'userIT1', N'User IT 1', N'0xDFA66240CF9508F1C806FB625BF80416C4555C607D18E7AFFAFADEFDE59AFF', N'0001', N'User', N'userIT1@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'userprod1', N'User Production 1', N'0x328A59C26DECDC6C0106251002C695B85CA004B1BCFBA15F1F5548B640C8DF', N'0004', N'User', N'userprod1@gmail.com', 1, 0)
INSERT [dbo].[userList] ([userID], [userName], [userPassword], [deptCode], [rankCode], [userEmail], [userIsActive], [userFailedLoginAttempt]) VALUES (N'userSCM 1', N'User SCM 1', N'0x7319CEC3CC371A0F43CB003AA887078390939981DB6CE00BD270C4265ED6C0', N'0003', N'User', N'userscm1@gmail.com', 1, 0)
INSERT [dbo].[weeklyDayOff] ([weekDay]) VALUES (N'Saturday')
INSERT [dbo].[weeklyDayOff] ([weekDay]) VALUES (N'Sunday')
SET ANSI_PADDING ON

GO
/****** Object:  Index [UQ__userList__D54ADF5503743FB8]    Script Date: 1/13/2020 11:45:25 PM ******/
ALTER TABLE [dbo].[userList] ADD UNIQUE NONCLUSTERED 
(
	[userEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[userList]  WITH CHECK ADD  CONSTRAINT [FK_userList_deptList] FOREIGN KEY([deptCode])
REFERENCES [dbo].[deptList] ([deptCode])
GO
ALTER TABLE [dbo].[userList] CHECK CONSTRAINT [FK_userList_deptList]
GO
ALTER TABLE [dbo].[userList]  WITH CHECK ADD  CONSTRAINT [FK_userList_rankList] FOREIGN KEY([rankCode])
REFERENCES [dbo].[rankList] ([rankCode])
GO
ALTER TABLE [dbo].[userList] CHECK CONSTRAINT [FK_userList_rankList]
GO
/****** Object:  StoredProcedure [dbo].[appWaitingApproval]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[appWaitingApproval]
	@userId nvarchar(20),
	@recCount bigint out
AS
IF NOT EXISTS (SELECT * FROM userList WHERE userID = @userId and userIsActive = 1)
BEGIN
	SET @recCount = 0
	RETURN
END;

DECLARE @rankCode nvarchar(20), @deptCode nvarchar(20)
SELECT @rankCode = rankCode, @deptCode = deptCode from userList WHERE userID = @userId


IF @rankCode = 'User'
BEGIN
	SET @recCount = 0
	RETURN
END;

IF @rankCode = 'Manager'
BEGIN
	SET @recCount = (SELECT COUNT(*) FROM leaveApplicationData
		WHERE 1 = 1
		AND deptCode = @deptCode
		AND rankCode = 'User'
		AND validationStatus = 'validated'
		AND applicationProgress = 'submitted'
		AND systemStatus = 'normal')

	SELECT t1.*, t2.reasonDetail, t3.deptName, t4.userName, t5.rankDescription FROM leaveApplicationData t1
	INNER JOIN [mData-LeaveReason] t2 on t1.reasonCode = t2.reasonCode
	INNER JOIN deptList t3 on t1.deptCode = t3.deptCode
	INNER JOIN userList t4 on t1.userID = t4.userID
	INNER JOIN rankList t5 on t4.rankCode = t5.rankCode
		WHERE 1 = 1
		AND t1.deptCode = @deptCode
		AND t1.rankCode = 'User'
		AND t1.validationStatus = 'validated'
		AND t1.applicationProgress = 'submitted'
		AND t1.systemStatus = 'normal'
	RETURN
END;

IF @rankCode = 'Director'
BEGIN
	SET @recCount = (SELECT COUNT(*) FROM leaveApplicationData
		WHERE 1 = 1
		AND rankCode = 'Manager'
		AND validationStatus = 'validated'
		AND applicationProgress = 'submitted'
		AND systemStatus = 'normal')

	SELECT t1.*, t2.reasonDetail, t3.deptName, t4.userName, t5.rankDescription FROM leaveApplicationData t1
	INNER JOIN [mData-LeaveReason] t2 on t1.reasonCode = t2.reasonCode
	INNER JOIN deptList t3 on t1.deptCode = t3.deptCode
	INNER JOIN userList t4 on t1.userID = t4.userID
	INNER JOIN rankList t5 on t4.rankCode = t5.rankCode
		WHERE 1 = 1
		AND t1.rankCode = 'Manager'
		AND t1.validationStatus = 'validated'
		AND t1.applicationProgress = 'submitted'
		AND t1.systemStatus = 'normal'
	RETURN
END;

IF @rankCode = 'Admin'
BEGIN
	SET @recCount = (SELECT COUNT(*) FROM leaveApplicationData
		WHERE 1 = 1
		AND validationStatus = 'validated'
		AND applicationProgress = 'submitted'
		AND systemStatus = 'normal')

	SELECT t1.*, t2.reasonDetail, t3.deptName, t4.userName, t5.rankDescription FROM leaveApplicationData t1
	INNER JOIN [mData-LeaveReason] t2 on t1.reasonCode = t2.reasonCode
	INNER JOIN deptList t3 on t1.deptCode = t3.deptCode
	INNER JOIN userList t4 on t1.userID = t4.userID
	INNER JOIN rankList t5 on t4.rankCode = t5.rankCode
		WHERE 1 = 1
		AND t1.validationStatus = 'validated'
		AND t1.applicationProgress = 'submitted'
		AND t1.systemStatus = 'normal'
	RETURN
END;
GO
/****** Object:  StoredProcedure [dbo].[authorizeApp]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[authorizeApp]
	@userID nvarchar(20),
	@appID nvarchar(20),
	@approverDesc nvarchar(200),
	@instruction int,
	@result int out
AS
SET @result = 0
IF NOT EXISTS (SELECT * FROM userList WHERE userID = @userID) RETURN
DECLARE @rankCode nvarchar(20), @deptCode nvarchar(20)
SELECT @rankCode = rankCode, @deptCode = deptCode from userList WHERE userID = @userId
IF @rankCode = 'User' RETURN
DECLARE @rankCode1 nvarchar(20), @deptCode1 nvarchar(20)

IF NOT EXISTS (SELECT * FROM leaveApplicationData 
				WHERE appID = @appID
				AND applicationProgress = 'Submitted'
				AND systemStatus = 'normal'
				AND validationStatus = 'validated') RETURN
SELECT @rankCode1 = rankCode, @deptCode1 = deptCode from leaveApplicationData WHERE appID = @appID

DECLARE @action nvarchar(20)
IF @instruction = 1 SET @action = 'Approve' ELSE SET @action = 'Reject'
IF @rankCode = 'Admin'
	BEGIN
	UPDATE t1 SET t1.approverAction = @action,
			t1.approverUserID = @userID,
			t1.applicationProgress = 'Done',
			t1.approverDesc = @approverDesc
			FROM leaveApplicationData t1
			WHERE appID = @appID
	SET @result = 1
	RETURN
	END

IF @rankCode = 'Director'
	BEGIN
	IF @rankCode1 <> 'Manager'
		RETURN
		ELSE
			BEGIN
				UPDATE t1 SET t1.approverAction = @action,
					t1.approverUserID = @userID,
					t1.applicationProgress = 'done',
					t1.approverDesc = @approverDesc
					FROM leaveApplicationData t1
					WHERE appID = @appID
				SET @result = 1
				RETURN
			END
	END

IF @rankCode = 'Manager'
	BEGIN
		IF @rankCode1 <> 'User' OR @deptCode1 <> @deptCode RETURN
			ELSE
			BEGIN
				UPDATE t1 SET t1.approverAction = @action,
					t1.approverUserID = @userID,
					t1.applicationProgress = 'Done',
					t1.approverDesc = @approverDesc
					FROM leaveApplicationData t1
					WHERE appID = @appID
				SET @result = 1
				RETURN
			END
	END
GO
/****** Object:  StoredProcedure [dbo].[DisablePublicHoliday]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DisablePublicHoliday]
	@holiday nvarchar(20),
	@result int out
AS
declare @rowversion nvarchar(30)
if not exists( SELECT * FROM publicHoliday WHERE holiday = @holiday)
	BEGIN
		SET @result = 0
		RETURN
	END

if exists( SELECT * FROM leaveApplicationData 
		WHERE left(timeStart,10) <= @holiday 
			AND left(timeEnd,10) >= @holiday
			AND (applicationProgress = 'submitted' OR (approverAction = 'Approve' AND systemStatus = 'normal'))
			)
	BEGIN
		SET @result = -1
		RETURN
	END

SET @rowversion = convert(NVARCHAR(30), CURRENT_TIMESTAMP, 127)
UPDATE t1 set t1.holiday = t1.holiday + ' disabled at ' + @rowversion
FROM publicHoliday t1
WHERE t1.holiday = @holiday
SET @result = 1
GO
/****** Object:  StoredProcedure [dbo].[GetPublicHolidayList]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPublicHolidayList]

AS
SELECT * FROM publicHoliday WHERE len(holiday) = 10
GO
/****** Object:  StoredProcedure [dbo].[InsertPublicHoliday]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertPublicHoliday]
	@holiday nvarchar(20),
	@description nvarchar(100),
	@result int out
AS
if exists( SELECT * FROM publicHoliday WHERE holiday = @holiday)
	BEGIN
		SET @result = 0
		RETURN
	END

if exists( SELECT * FROM leaveApplicationData 
		WHERE left(timeStart,10) <= @holiday 
			AND left(timeEnd,10) >= @holiday
			AND (applicationProgress = 'submitted' OR (approverAction = 'Approve' AND systemStatus = 'normal'))
			)
	BEGIN
		SET @result = -1
		RETURN
	END
INSERT INTO publicHoliday(holiday, description) VALUES (@holiday, @description)
SET @result = 1
GO
/****** Object:  StoredProcedure [dbo].[ListActiveUser]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ListActiveUser]
AS
	SELECT t1.userID, t1.userName, t1.deptCode, t2.deptName, t1.rankCode, t3.rankDescription, t1.userEmail, t1.userIsActive, t1.userFailedLoginAttempt
	from userList t1 
	inner join deptList t2 on t1.deptCode = t2.deptCode
	inner join rankList t3 on t1.rankCode = t3.rankCode
	where t1.userIsActive = 1
	order by t1.userID
GO
/****** Object:  StoredProcedure [dbo].[showBalance]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[showBalance]
	@userID nvarchar(20),
	@reportYear nvarchar(10)
AS
DECLARE @yearlyANNL nvarchar(20),
		@yearlyBRML nvarchar(20),
		@yearlyCPSL nvarchar(20),
		@yearlyFMRL nvarchar(20),
		@yearlySMRL nvarchar(20),
		@yearlySCKL nvarchar(20),
		@yearlyUPDL nvarchar(20),
		@yearlySPCL nvarchar(20)
SELECT @yearlyANNL = yearlyANNL,
		@yearlyBRML = yearlyBRML,
		@yearlyCPSL = yearlyCPSL,
		@yearlyFMRL = yearlyFMRL,
		@yearlySMRL = yearlySMRL,
		@yearlySCKL = yearlySCKL,
		@yearlyUPDL = yearlyUPDL,
		@yearlySPCL = yearlySPCL
		FROM [mData-LeaveQuota] WHERE userID = @userID;
DECLARE @balanceSummary TABLE
(
	reasonCode nvarchar(50),
	reasonDetail nvarchar(50),
	leaveQuota nvarchar(50),
	leaveUsed nvarchar(50),
	leavePending nvarchar(50),
	leaveBalance nvarchar(50)
)
DECLARE @activeYear nvarchar(10) = @reportYear

INSERT INTO @balanceSummary (reasonCode, reasonDetail) SELECT reasonCode, reasonDetail FROM [mData-LeaveReason]

UPDATE t1 SET t1.leaveQuota = @yearlyANNL FROM @balanceSummary t1 WHERE t1.reasonCode = 'ANNL'
UPDATE t1 SET t1.leaveQuota = @yearlyBRML FROM @balanceSummary t1 WHERE t1.reasonCode = 'BRML'
UPDATE t1 SET t1.leaveQuota = @yearlyCPSL FROM @balanceSummary t1 WHERE t1.reasonCode = 'CPSL'
UPDATE t1 SET t1.leaveQuota = @yearlyFMRL FROM @balanceSummary t1 WHERE t1.reasonCode = 'FMRL'
UPDATE t1 SET t1.leaveQuota = @yearlySMRL FROM @balanceSummary t1 WHERE t1.reasonCode = 'SMRL'
UPDATE t1 SET t1.leaveQuota = @yearlySCKL FROM @balanceSummary t1 WHERE t1.reasonCode = 'SCKL'
UPDATE t1 SET t1.leaveQuota = @yearlyUPDL FROM @balanceSummary t1 WHERE t1.reasonCode = 'UPDL'
UPDATE t1 SET t1.leaveQuota = @yearlySPCL FROM @balanceSummary t1 WHERE t1.reasonCode = 'SPCL'

UPDATE t1 SET t1.leaveBalance = 'Unlimited' FROM @balanceSummary t1 WHERE t1.leaveQuota = 'Unlimited'

UPDATE t1 SET t1.leaveUsed = cast(isnull(t2.total,0) as nvarchar(50))
FROM @balanceSummary t1
INNER JOIN 
(SELECT SUM(ISNULL(hourRequired, 0)) AS total , reasonCode 
	FROM leaveApplicationData 
	WHERE userID = @userID AND approverAction = 'Approve' 
	AND applicationProgress = 'Done' and systemStatus = 'normal'
	AND LEFT(timeStart,4) = @activeYear
	GROUP BY reasonCode) t2
ON t1.reasonCode = t2.reasonCode

UPDATE t1 SET t1.leaveUsed = '0'
FROM @balanceSummary t1
LEFT JOIN 
(SELECT SUM(ISNULL(hourRequired, 0)) AS total , reasonCode 
	FROM leaveApplicationData 
	WHERE userID = @userID AND approverAction = 'Approve' 
	AND applicationProgress = 'Done' and systemStatus = 'normal'
	AND LEFT(timeStart,4) = @activeYear
	GROUP BY reasonCode) t2
ON t1.reasonCode = t2.reasonCode
WHERE t2.reasonCode IS NULL

UPDATE t1 SET t1.leavePending = cast(isnull(t2.total,0) as nvarchar(50))
FROM @balanceSummary t1
INNER JOIN 
(SELECT SUM(ISNULL(hourRequired, 0)) AS total , reasonCode 
	FROM leaveApplicationData 
	WHERE userID = @userID
	AND applicationProgress = 'submitted' and systemStatus = 'normal'
	AND LEFT(timeStart,4) = @activeYear
	GROUP BY reasonCode) t2
ON t1.reasonCode = t2.reasonCode

UPDATE t1 SET t1.leavePending = '0'
FROM @balanceSummary t1
LEFT JOIN 
(SELECT SUM(ISNULL(hourRequired, 0)) AS total , reasonCode 
	FROM leaveApplicationData 
	WHERE userID = @userID
	AND applicationProgress = 'submitted' and systemStatus = 'normal'
	AND LEFT(timeStart,4) = @activeYear
	GROUP BY reasonCode) t2
ON t1.reasonCode = t2.reasonCode
WHERE t2.reasonCode IS NULL

UPDATE t1 SET t1.leaveBalance = 
cast(dbo.UdfMaxInt((cast(t1.leaveQuota as bigint) - 
cast(t1.leaveUsed as bigint) - 
cast(t1.leavePending as bigint)),0) as nvarchar(50))
FROM @balanceSummary t1
WHERE t1.leaveBalance IS NULL

SELECT * FROM @balanceSummary
GO
/****** Object:  StoredProcedure [dbo].[spChangePassword]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spChangePassword]
	@userID nvarchar(50),
	@oldPassword nvarchar(50),
	@newPassword nvarchar(50),
	@result int OUT
AS
SET NOCOUNT ON
SET @result = 0
IF EXISTS (SELECT * FROM userList t1 WHERE t1.userID = @userID AND t1.userPassword = dbo.HASHPW(@oldPassword, @userID))
	BEGIN
		UPDATE t1 SET t1.userPassword = dbo.HASHPW(@newPassword, @userID) FROM userList t1
			WHERE t1.userID = @userID;
		SET @result = 1
	END
RETURN
GO
/****** Object:  StoredProcedure [dbo].[spCreateNewUser]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spCreateNewUser]
    @userID nvarchar(50),  
    @userName nvarchar(50),
	@userPassword nvarchar(50),
	@deptCode nvarchar(50),
	@rankCode nvarchar(50),
	@userEmail nvarchar(50),
	@procResult int out
AS  
SET NOCOUNT ON;
BEGIN TRY
	BEGIN TRAN
	SELECT NULL userID FROM userList WITH (TABLOCKX)
	IF EXISTS (SELECT userEmail FROM userList WHERE userEmail = @userEmail)
		BEGIN
			ROLLBACK TRAN
			SELECT @procResult = 0
			RETURN
		END
	INSERT INTO userList (userID, userName, userPassword, 
							deptCode, rankCode, userEmail)
		VALUES (@userID, @userName, dbo.HASHPW(@userPassword,@userID), 
							@deptCode, @rankCode, @userEmail);
	SELECT @procResult = 1
	COMMIT TRAN
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;
	SELECT @procResult = ERROR_NUMBER();
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spEditUserInfo]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spEditUserInfo]
	@userID nvarchar(50),
	@userName nvarchar(50),
	@deptCode nvarchar(50),
	@rankCode nvarchar(50),
	@userEmail nvarchar(50),
	@userIsActive bit,
	@userFailedLoginAttempt int,
	@result int out
AS
SET NOCOUNT ON
DECLARE @status INT;
DECLARE @userEmailConflict INT;
IF EXISTS (SELECT * FROM userList WHERE userID = @userID)
	BEGIN
		SET @userEmailConflict = (SELECT COUNT(*) FROM userList WHERE userID <> @userID AND userEmail = @userEmail)
		PRINT @userEmailConflict
		IF @userEmailConflict > 0
			SET @status = 1
		ELSE
			BEGIN
				UPDATE userList SET userName = @userName, deptCode = @deptCode, rankCode = @rankCode,
					userEmail = @userEmail, userIsActive = @userIsActive, userFailedLoginAttempt = @userFailedLoginAttempt
				WHERE userID = @userID;
				SET @status = 2
			END
	END
ELSE
	SET @status = 0

SET @result = @status
GO
/****** Object:  StoredProcedure [dbo].[spInitLeaveApp]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spInitLeaveApp]
	@userID nvarchar(50)
AS
set nocount on
declare @appID nvarchar(50)
if not exists (select * from leaveApplicationData t1 where t1.applicationProgress = 'initialized'
								and t1.userID = @userID)
begin
	set @appID = dbo.udfInitAppID();
	insert into leaveApplicationData(appID, userID, initDate, validationStatus, applicationProgress, systemStatus)
		values (@appID, @userID, GETDATE(), 'not validated', 'initialized', 'normal');
	update t1 set t1.deptCode = t2.deptCode, t1.rankCode = t2.rankCode
	from leaveApplicationData t1 inner join userList t2 on t1.userID = t2.userID
	where t1.appID = @appID;
end
select t1.*, t2.userName, t3.deptName, t4.rankDescription  from leaveApplicationData t1 
	inner join userList t2 on t1.userID = t2.userID
	inner join deptList t3 on t1.deptCode = t3.deptCode
	inner join rankList t4 on t1.rankCode = t4.rankCode
	where t1.applicationProgress = 'initialized' and t1.userID = @userID;
GO
/****** Object:  StoredProcedure [dbo].[spLeaveQuotaDetail]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spLeaveQuotaDetail]
	@userID nvarchar(50),
	@recordCount int out
AS
SET NOCOUNT ON;
SET @recordCount = (select count(*) from [mData-LeaveQuota] t1 
					inner join userList t2 on t1.userID = t2.userID
					where t2.rankCode <> 'Admin' and t1.userID = @userID);
IF @recordCount = 0
	RETURN;

select t1.userID, t2.userName, t1.yearlyANNL, t1.yearlyBRML, t1.yearlyCPSL, t1.yearlyFMRL,
	t1.yearlySCKL, t1.yearlySMRL, t1.yearlySPCL, t1.yearlyUPDL
from [mData-LeaveQuota] t1			
inner join userList t2 on t1.userID = t2.userID
where t2.rankCode <> 'Admin' and t1.userID = @userID
GO
/****** Object:  StoredProcedure [dbo].[spLeaveQuotaSummary]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spLeaveQuotaSummary]
	@pageSize int,
	@pageIndex int,
	@recordCount bigint out
AS
BEGIN
--
--
SET NOCOUNT ON;
DECLARE @skipRows int;
SET @recordCount = (select count(*) from [mData-LeaveQuota] t1 
					inner join userList t2 on t1.userID = t2.userID
					where t2.rankCode <> 'Admin');

IF @recordCount = 0
	RETURN;
IF @pageIndex <=0
	SET @pageIndex = 1;
IF @pageSize <=0
	SET @pageIndex = @recordCount;
If @pageIndex*@pageSize > @recordCount
	SET @pageIndex = CEILING(Convert(Decimal(10,0),@recordCount)/Convert(Decimal(10,0),@pageSize));
SET @skipRows = (@pageIndex - 1)*@pageSize;
SELECT t1.userID, t2.userName, t1.yearlyANNL, t1.yearlyBRML, t1.yearlyCPSL, t1.yearlyFMRL, t1.yearlySMRL, t1.yearlySCKL, t1.yearlySPCL, t1.yearlyUPDL
	from [mData-LeaveQuota] t1 
	inner join userList t2 on t1.userID = t2.userID
	where t2.rankCode <> 'Admin'
	order by t1.userID
	offset @skipRows rows
	fetch next @pageSize rows only;
--
--
END
GO
/****** Object:  StoredProcedure [dbo].[spListAllApplication]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spListAllApplication]
	@pageSize int,
	@pageIndex int,
	@recordCount bigint out
AS
BEGIN
--
--
SET NOCOUNT ON;
DECLARE @skipRows int;
SET @recordCount = (select count(*) from leaveApplicationData t1)

IF @recordCount = 0
	RETURN;
IF @pageIndex <=0
	SET @pageIndex = 1;
IF @pageSize <=0
	SET @pageIndex = @recordCount;
If @pageIndex*@pageSize > @recordCount
	SET @pageIndex = CEILING(Convert(Decimal(10,0),@recordCount)/Convert(Decimal(10,0),@pageSize));
SET @skipRows = (@pageIndex - 1)*@pageSize;

SELECT t1.*, t2.reasonDetail, t3.deptName, t4.userName, t5.rankDescription FROM leaveApplicationData t1
LEFT JOIN [mData-LeaveReason] t2 on t1.reasonCode = t2.reasonCode
INNER JOIN deptList t3 on t1.deptCode = t3.deptCode
INNER JOIN userList t4 on t1.userID = t4.userID
INNER JOIN rankList t5 on t4.rankCode = t5.rankCode
	WHERE 1 = 1

	order by t1.appID
	offset @skipRows rows
	fetch next @pageSize rows only;
--
--
END
GO
/****** Object:  StoredProcedure [dbo].[spListAllApplicationWithFilter]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spListAllApplicationWithFilter]
	@pageSize int,
	@pageIndex int,
	@recordCount bigint out,
	@check0 nvarchar(max), @check1 nvarchar(max), @check2 nvarchar(max), @check3 nvarchar(max), @check4 nvarchar(max), @check5 nvarchar(max),
	@op0 nvarchar(max), @op1 nvarchar(max), @op2 nvarchar(max), @op3 nvarchar(max), @op4 nvarchar(max), @op5 nvarchar(max),
	@criteria0 nvarchar(max), @criteria1 nvarchar(max), @criteria2 nvarchar(max), @criteria3 nvarchar(max), @criteria4 nvarchar(max), @criteria5 nvarchar(max)
AS
BEGIN
--
--
SET NOCOUNT ON;
DECLARE @temp TABLE(
	[appID]               NVARCHAR (50)  NOT NULL,
    [initDate]            DATE           NULL,
    [userID]              NVARCHAR (50)  NULL,
    [deptCode]            NVARCHAR (50)  NULL,
    [rankCode]            NVARCHAR (50)  NULL,
    [reasonCode]          NVARCHAR (50)  NULL,
    [applicantDesc]       NVARCHAR (200) NULL,
    [timeStart]           NVARCHAR (50)  NULL,
    [timeEnd]             NVARCHAR (50)  NULL,
    [hourRequired]        INT            NULL,
    [validationStatus]    NVARCHAR (50)  NULL,
    [approverUserID]      NVARCHAR (50)  NULL,
    [approverAction]      NVARCHAR (50)  NULL,
    [approverDesc]        NVARCHAR (200) NULL,
    [applicationProgress] NVARCHAR (50)  NULL,
    [systemStatus]        NVARCHAR (50)  NULL,
    [recordChangeLog]     NVARCHAR (50)  NULL,
	[reasonDetail]	NVARCHAR (50)  NULL,
	[deptName]	NVARCHAR (50)  NULL,
	[userName]	NVARCHAR (50)  NULL,
	[rankDescription]	NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([appID] ASC)
);

DECLARE @cond0 nvarchar(max), @cond1 nvarchar(max), @cond2 nvarchar(max), @cond3 nvarchar(max), @cond4 nvarchar(max), @cond5 nvarchar(max)

SET @cond0 = '';
SET @cond1 = '';
SET @cond2 = '';
SET @cond3 = '';
SET @cond4 = '';
SET @cond5 = '';

if @check0 = 'True' SET @cond0 = ' AND t1.appID' + dbo.buildCondition(@op0, @criteria0)
if @check1 = 'True' SET @cond1 = ' AND t1.deptCode' + dbo.buildCondition(@op1, @criteria1)
if @check2 = 'True' SET @cond2 = ' AND t4.userName' + dbo.buildCondition(@op2, @criteria2)
if @check3 = 'True' SET @cond3 = ' AND coalesce(t1.approverAction, '''')' + dbo.buildCondition(@op3, @criteria3)
if @check4 = 'True' SET @cond4 = ' AND t1.applicationProgress' + dbo.buildCondition(@op4, @criteria4)
if @check5 = 'True' SET @cond5 = ' AND t1.systemStatus' + dbo.buildCondition(@op5, @criteria5)

DECLARE @sql nvarchar(max);

SET @sql = 'SELECT t1.*,
			t2.reasonDetail, t3.deptName, t4.userName, t5.rankDescription FROM leaveApplicationData t1'
		+ ' LEFT JOIN [mData-LeaveReason] t2 on t1.reasonCode = t2.reasonCode'
		+ ' INNER JOIN deptList t3 on t1.deptCode = t3.deptCode'
		+ ' INNER JOIN userList t4 on t1.userID = t4.userID'
		+ ' INNER JOIN rankList t5 on t4.rankCode = t5.rankCode'
		+ ' WHERE 1 = 1'
		+ @cond0 + @cond1 + @cond2 + @cond3 + @cond4 + @cond5;

INSERT into @temp EXEC(@sql);


DECLARE @skipRows int;
SET @recordCount = (select count(*) from @temp t1)
IF @recordCount = 0
	RETURN;
IF @pageIndex <=0
	SET @pageIndex = 1;
IF @pageSize <=0
	SET @pageIndex = @recordCount;
If @pageIndex*@pageSize > @recordCount
	SET @pageIndex = CEILING(Convert(Decimal(10,0),@recordCount)/Convert(Decimal(10,0),@pageSize));
SET @skipRows = (@pageIndex - 1)*@pageSize;

SELECT t1.* FROM @temp t1

	WHERE 1 = 1

	order by t1.appID
	offset @skipRows rows
	fetch next @pageSize rows only;
--
--
END
GO
/****** Object:  StoredProcedure [dbo].[spListAllApplicationWithFilterForDownload]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spListAllApplicationWithFilterForDownload]
	@pageSize int,
	@pageIndex int,
	@recordCount bigint out,
	@check0 nvarchar(max), @check1 nvarchar(max), @check2 nvarchar(max), @check3 nvarchar(max), @check4 nvarchar(max), @check5 nvarchar(max),
	@op0 nvarchar(max), @op1 nvarchar(max), @op2 nvarchar(max), @op3 nvarchar(max), @op4 nvarchar(max), @op5 nvarchar(max),
	@criteria0 nvarchar(max), @criteria1 nvarchar(max), @criteria2 nvarchar(max), @criteria3 nvarchar(max), @criteria4 nvarchar(max), @criteria5 nvarchar(max)
AS
BEGIN
--
--
SET NOCOUNT ON;
DECLARE @temp TABLE(
	[appID]               NVARCHAR (50)  NOT NULL,
    [initDate]            DATE           NULL,
    [userID]              NVARCHAR (50)  NULL,
    [deptCode]            NVARCHAR (50)  NULL,
    [rankCode]            NVARCHAR (50)  NULL,
    [reasonCode]          NVARCHAR (50)  NULL,
    [applicantDesc]       NVARCHAR (200) NULL,
    [timeStart]           NVARCHAR (50)  NULL,
    [timeEnd]             NVARCHAR (50)  NULL,
    [hourRequired]        INT            NULL,
    [validationStatus]    NVARCHAR (50)  NULL,
    [approverUserID]      NVARCHAR (50)  NULL,
    [approverAction]      NVARCHAR (50)  NULL,
    [approverDesc]        NVARCHAR (200) NULL,
    [applicationProgress] NVARCHAR (50)  NULL,
    [systemStatus]        NVARCHAR (50)  NULL,
    [recordChangeLog]     NVARCHAR (50)  NULL,
	[reasonDetail]	NVARCHAR (50)  NULL,
	[deptName]	NVARCHAR (50)  NULL,
	[userName]	NVARCHAR (50)  NULL,
	[rankDescription]	NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([appID] ASC)
);

DECLARE @cond0 nvarchar(max), @cond1 nvarchar(max), @cond2 nvarchar(max), @cond3 nvarchar(max), @cond4 nvarchar(max), @cond5 nvarchar(max)

SET @cond0 = '';
SET @cond1 = '';
SET @cond2 = '';
SET @cond3 = '';
SET @cond4 = '';
SET @cond5 = '';

if @check0 = 'True' SET @cond0 = ' AND t1.appID' + dbo.buildCondition(@op0, @criteria0)
if @check1 = 'True' SET @cond1 = ' AND t1.deptCode' + dbo.buildCondition(@op1, @criteria1)
if @check2 = 'True' SET @cond2 = ' AND t4.userName' + dbo.buildCondition(@op2, @criteria2)
if @check3 = 'True' SET @cond3 = ' AND coalesce(t1.approverAction, '''')' + dbo.buildCondition(@op3, @criteria3)
if @check4 = 'True' SET @cond4 = ' AND t1.applicationProgress' + dbo.buildCondition(@op4, @criteria4)
if @check5 = 'True' SET @cond5 = ' AND t1.systemStatus' + dbo.buildCondition(@op5, @criteria5)

DECLARE @sql nvarchar(max);

SET @sql = 'SELECT t1.*,
			t2.reasonDetail, t3.deptName, t4.userName, t5.rankDescription FROM leaveApplicationData t1'
		+ ' LEFT JOIN [mData-LeaveReason] t2 on t1.reasonCode = t2.reasonCode'
		+ ' INNER JOIN deptList t3 on t1.deptCode = t3.deptCode'
		+ ' INNER JOIN userList t4 on t1.userID = t4.userID'
		+ ' INNER JOIN rankList t5 on t4.rankCode = t5.rankCode'
		+ ' WHERE 1 = 1'
		+ @cond0 + @cond1 + @cond2 + @cond3 + @cond4 + @cond5;

INSERT into @temp EXEC(@sql);


DECLARE @skipRows int;
SET @recordCount = (select count(*) from @temp t1)
IF @recordCount = 0
	RETURN;
IF @pageIndex <=0
	SET @pageIndex = 1;
IF @pageSize <=0
	SET @pageIndex = @recordCount;
If @pageIndex*@pageSize > @recordCount
	SET @pageIndex = CEILING(Convert(Decimal(10,0),@recordCount)/Convert(Decimal(10,0),@pageSize));
SET @skipRows = (@pageIndex - 1)*@pageSize;

SELECT t1.* FROM @temp t1
	WHERE 1 = 1
	order by t1.appID

--
--
END
GO
/****** Object:  StoredProcedure [dbo].[spListAllUsers]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spListAllUsers]
	--@param1 int = 0,
	--@param2 int
AS
	SELECT t1.userID, t1.userName, t1.deptCode, t2.deptName, t1.rankCode, t3.rankDescription, t1.userEmail, t1.userIsActive, t1.userFailedLoginAttempt
	from userList t1 
	inner join deptList t2 on t1.deptCode = t2.deptCode
	inner join rankList t3 on t1.rankCode = t3.rankCode
	order by t1.userID
GO
/****** Object:  StoredProcedure [dbo].[spListAllUsersWithPagination]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spListAllUsersWithPagination]
	@pageSize int,
	@pageIndex int,
	@recordCount bigint out
AS
BEGIN
--
--
SET NOCOUNT ON;
DECLARE @skipRows int;
SET @recordCount = (select count(*) from userList where rankCode <> 'Admin');

IF @recordCount = 0
	RETURN;
IF @pageIndex <=0
	SET @pageIndex = 1;
IF @pageSize <=0
	SET @pageIndex = @recordCount;
If @pageIndex*@pageSize > @recordCount
	SET @pageIndex = CEILING(Convert(Decimal(10,0),@recordCount)/Convert(Decimal(10,0),@pageSize));
SET @skipRows = (@pageIndex - 1)*@pageSize;
SELECT t1.userID, t1.userName, t1.deptCode, t2.deptName, t1.rankCode, t3.rankDescription, t1.userEmail, t1.userIsActive, t1.userFailedLoginAttempt
	from userList t1 
	inner join deptList t2 on t1.deptCode = t2.deptCode
	inner join rankList t3 on t1.rankCode = t3.rankCode
	where t1.rankCode <> 'Admin'
	order by t1.userID
	offset @skipRows rows
	fetch next @pageSize rows only;
--
--
END
GO
/****** Object:  StoredProcedure [dbo].[spListMyApplication]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spListMyApplication]
	@pageSize int,
	@pageIndex int,
	@userID nvarchar(50),
	@recordCount bigint out
AS
BEGIN
--
--
SET NOCOUNT ON;
DECLARE @skipRows int;
SET @recordCount = (select count(*) from leaveApplicationData t1 where userID = @userID)

IF @recordCount = 0
	RETURN;
IF @pageIndex <=0
	SET @pageIndex = 1;
IF @pageSize <=0
	SET @pageIndex = @recordCount;
If @pageIndex*@pageSize > @recordCount
	SET @pageIndex = CEILING(Convert(Decimal(10,0),@recordCount)/Convert(Decimal(10,0),@pageSize));
SET @skipRows = (@pageIndex - 1)*@pageSize;

SELECT t1.*, t2.reasonDetail, t3.deptName, t4.userName, t5.rankDescription FROM leaveApplicationData t1
LEFT JOIN [mData-LeaveReason] t2 on t1.reasonCode = t2.reasonCode
INNER JOIN deptList t3 on t1.deptCode = t3.deptCode
INNER JOIN userList t4 on t1.userID = t4.userID
INNER JOIN rankList t5 on t4.rankCode = t5.rankCode
	WHERE 1 = 1
	and t1.userID = @userID
	order by t1.appID desc
	offset @skipRows rows
	fetch next @pageSize rows only;
--
--
END
GO
/****** Object:  StoredProcedure [dbo].[spListMyApplicationForDownload]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spListMyApplicationForDownload]
	@pageSize int,
	@pageIndex int,
	@userID nvarchar(50),
	@recordCount bigint out
AS
BEGIN
--
--
SET NOCOUNT ON;
DECLARE @skipRows int;
SET @recordCount = (select count(*) from leaveApplicationData t1 where userID = @userID)

IF @recordCount = 0
	RETURN;
IF @pageIndex <=0
	SET @pageIndex = 1;
IF @pageSize <=0
	SET @pageIndex = @recordCount;
If @pageIndex*@pageSize > @recordCount
	SET @pageIndex = CEILING(Convert(Decimal(10,0),@recordCount)/Convert(Decimal(10,0),@pageSize));
SET @skipRows = (@pageIndex - 1)*@pageSize;

SELECT t1.*, t2.reasonDetail, t3.deptName, t4.userName, t5.rankDescription FROM leaveApplicationData t1
LEFT JOIN [mData-LeaveReason] t2 on t1.reasonCode = t2.reasonCode
INNER JOIN deptList t3 on t1.deptCode = t3.deptCode
INNER JOIN userList t4 on t1.userID = t4.userID
INNER JOIN rankList t5 on t4.rankCode = t5.rankCode
	WHERE 1 = 1
	and t1.userID = @userID
	order by t1.appID desc
--
--
END
GO
/****** Object:  StoredProcedure [dbo].[spUpdateLeaveQuota]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spUpdateLeaveQuota]
	@userID nvarchar(50),
	@yearlyANNL nvarchar(50),
	@yearlyBRML nvarchar(50),
	@yearlyCPSL nvarchar(50),
	@yearlyFMRL nvarchar(50),
	@yearlySMRL nvarchar(50),
	@yearlySCKL nvarchar(50),
	@yearlyUPDL nvarchar(50),
	@yearlySPCL nvarchar(50),
	@result int out

AS
SET NOCOUNT ON;
IF NOT EXISTS (SELECT t1.* from [mData-LeaveQuota] t1 
					inner join userList t2 on t1.userID = t2.userID
					where t2.rankCode <> 'Admin' and t1.userID = @userID)
	GOTO resultZero;

UPDATE t1 SET	t1.yearlyANNL = @yearlyANNL,
				t1.yearlyBRML = @yearlyBRML,
				t1.yearlyCPSL = @yearlyCPSL,
				t1.yearlyFMRL = @yearlyFMRL,
				t1.yearlySCKL = @yearlySCKL,
				t1.yearlySMRL = @yearlySMRL,
				t1.yearlySPCL = @yearlySPCL,
				t1.yearlyUPDL = @yearlyUPDL

FROM [mData-LeaveQuota] t1
WHERE t1.userID = @userID;
SET @result = 1;
RETURN

resultZero:
SET @result = 0
GO
/****** Object:  StoredProcedure [dbo].[spUpdatePasswordByAdmin]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spUpdatePasswordByAdmin]
	@targetAccount nvarchar(50),
	@adminUser nvarchar(50),
	@adminPassword nvarchar(50),
	@newPassword nvarchar(50),
	@result int out
AS
set nocount on;
IF NOT EXISTS (SELECT * FROM userList WHERE userID = @targetAccount and userID <> 'sysadmin')
	GOTO invalidTarget;

IF NOT EXISTS (SELECT * FROM userList WHERE userPassword = dbo.HASHPW(@adminPassword, @adminUser) and rankCode = 'Admin')
	GOTO invalidAuthorization;

UPDATE t1 SET t1.userPassword = dbo.HASHPW(@newPassword, @targetAccount) FROM userList t1 WHERE t1.userID = @targetAccount;
SET @result = 2;
return;

invalidTarget:
	SET @result = 0;
	return;
invalidAuthorization:
	SET @result = 1;
	return;
GO
/****** Object:  StoredProcedure [dbo].[spUserIdentityAuth]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spUserIdentityAuth]
    @username nvarchar(50),  
    @password nvarchar(50),
	@validationResult BIT OUT,
	@role nvarchar(50) OUT,
	@Name nvarchar(50) OUT
AS  
SET NOCOUNT ON  
Declare @failed_count AS INT  
 

IF EXISTS(SELECT * FROM userList WHERE userID = @username)
	BEGIN
		SET @failed_count = (SELECT t1.userFailedLoginAttempt from userList as t1 WHERE t1.userID = @username);  
		IF @failed_count >= 5
			BEGIN
				SET @validationResult = 0
				RETURN
			END
		ELSE
			BEGIN  
				IF EXISTS(SELECT t1.* FROM userList as t1 WHERE t1.userID = @username 
							AND t1.userPassword = dbo.HASHPW(@password, @username) AND t1.userIsActive = 1)
					BEGIN
						SET @validationResult = 1
						SET @role = (SELECT t1.rankCode from userList AS t1 WHERE t1.userID = @username)
						SET @Name = (SELECT t1.userName from userList AS t1 WHERE t1.userID = @username)
						Update t1 set  t1.userFailedLoginAttempt = 0 FROM userList AS t1  
										WHERE t1.userID = @username;
						RETURN
					END
				ELSE
					BEGIN
						SET @validationResult = 0
						IF @failed_count < 5
						Update t1 set t1.userFailedLoginAttempt = @failed_count + 1 
								FROM userList AS t1  WHERE t1.userID = @username;
					END
			END
	END
ELSE
	BEGIN
		SET @validationResult = 0
		RETURN
	END
GO
/****** Object:  StoredProcedure [dbo].[submitLeaveApplication]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[submitLeaveApplication]
	@appID nvarchar(50),
	@userID nvarchar(50),
	@reasonCode nvarchar(50),
	@applicantDesc nvarchar(200),
	@timeStart nvarchar(50),
	@timeEnd nvarchar(50),
	@result int out
AS
set nocount on;
declare @hourRequired int;
declare @validStatus bit;
if not exists (select * from leaveApplicationData 
				where appID = @appID and userID = @userID and applicationProgress = 'initialized' and systemStatus = 'normal')
				begin
				set @result = -1 -- không tìm thấy yêu cầu nghỉ phép
				end;
update t1 set t1.timeStart = @timeStart, t1.timeEnd = @timeEnd, 
		t1.hourRequired = dbo.calculateWorkHour(@timeStart, @timeEnd),
		t1.applicantDesc = @applicantDesc,
		t1.reasonCode = @reasonCode
		from leaveApplicationData t1
		where t1.appID = @appID;
set @validStatus = dbo.validateApp(@appID);
IF @validStatus = 1
begin
	update t1 SET t1.applicationProgress = 'submitted',
		t1.validationStatus = 'validated'
		from leaveApplicationData t1
		where t1.appID = @appID;
	set @result = 1;
end
else
	set @result = 0;
GO
/****** Object:  StoredProcedure [dbo].[submitLeaveApplicationByAdmin]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[submitLeaveApplicationByAdmin]
	@userID nvarchar(50),
	@reasonCode nvarchar(50),
	@applicantDesc nvarchar(200),
	@timeStart nvarchar(50),
	@timeEnd nvarchar(50),
	@result int out
AS
set nocount on;
declare @hourRequired int;
declare @validStatus bit;
declare @appID nvarchar(50)
if not exists (select * from userList 
				where userID = @userID)
		set @result = -1;

if not exists (select * from [mData-LeaveReason] 
				where reasonCode = @reasonCode)
		set @result = -1;

set @appID = dbo.udfInitAppID();

begin transaction
insert into leaveApplicationData(appID, userID, initDate, validationStatus, applicationProgress, systemStatus, approverUserID, approverAction)
		values (@appID, @userID, GETDATE(), 'not validated', 'Done', 'normal', 'sysadmin', 'Approve');
update t1 set t1.deptCode = t2.deptCode, t1.rankCode = t2.rankCode
	from leaveApplicationData t1 inner join userList t2 on t1.userID = t2.userID
	where t1.appID = @appID;
update t1 set t1.timeStart = @timeStart, t1.timeEnd = @timeEnd, 
		t1.hourRequired = dbo.calculateWorkHour(@timeStart, @timeEnd),
		t1.applicantDesc = @applicantDesc,
		t1.reasonCode = @reasonCode
		from leaveApplicationData t1
		where t1.appID = @appID;
set @validStatus = dbo.validateApp(@appID);
IF @validStatus = 1
begin
	update t1 SET
		t1.validationStatus = 'validated'
		from leaveApplicationData t1
		where t1.appID = @appID;
	set @result = 1;
	commit transaction
end
else
begin
	rollback transaction;
	set @result = 0;
end
GO
/****** Object:  StoredProcedure [dbo].[terminateApp]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[terminateApp]
	@appID nvarchar(20),
	@result int out
AS

IF NOT EXISTS (SELECT * FROM leaveApplicationData WHERE appID = @appID
				AND (
						applicationProgress = 'initialized'
					OR	applicationProgress = 'submitted'
					OR	(approverAction = 'Approve' and systemStatus = 'normal')
					)
				)
	GOTO terminateSP;

UPDATE t1
SET t1.systemStatus = 'disabled', t1.applicationProgress = 'Done'
FROM leaveApplicationData t1
WHERE t1.appID = @appID;
SET @result = 1
RETURN;

terminateSP:
set @result = 0
return
GO
/****** Object:  Trigger [dbo].[userList_afterInsertDelete]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER [dbo].[userList_afterInsertDelete]
	ON [dbo].[userList]
	FOR DELETE, INSERT
	AS
	BEGIN
		SET NOCOUNT ON;
		INSERT INTO [mData-LeaveQuota](userID) SELECT t1.userID FROM inserted t1;
		DELETE [mData-LeaveQuota] FROM [mData-LeaveQuota] t1
		INNER JOIN deleted t2 ON t1.userID = t2.userID;
	END
GO
/****** Object:  Trigger [dbo].[userList_afterUpdate]    Script Date: 1/13/2020 11:45:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER [dbo].[userList_afterUpdate]
	ON [dbo].[userList]
	AFTER UPDATE
AS
	BEGIN
	SET NOCOUNT ON
		DECLARE @deptCode_old nvarchar(50);
		DECLARE @deptCode_new nvarchar(50);
		DECLARE @rankCode_old nvarchar(50);
		DECLARE @rankCode_new nvarchar(50);

		SET NOCOUNT ON;

		SET @deptCode_old = (SELECT deptCode FROM deleted);
		SET @deptCode_new = (SELECT deptCode FROM inserted);
		SET @rankCode_old = (SELECT rankCode FROM deleted);
		SET @rankCode_new = (SELECT rankCode FROM inserted);

		IF @deptCode_old <> @deptCode_new OR @rankCode_old <> @rankCode_new
			UPDATE t1 SET t1.applicationProgress = 'Done', t1.systemStatus = 'disabled' 
			FROM leaveApplicationData t1
			WHERE t1.applicationProgress <> 'Done' AND t1.systemStatus <> 'disabled';
	END
GO
