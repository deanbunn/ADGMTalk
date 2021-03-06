USE [COEADGroupManager]
GO
/****** Object:  Table [dbo].[AdminGrpGuids]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminGrpGuids](
	[AGGID] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AGGID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AGMGroup]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AGMGroup](
	[AGMGID] [uniqueidentifier] NOT NULL,
	[ADGrpName] [nvarchar](400) NULL,
	[ADGrpDN] [nvarchar](2000) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [nvarchar](200) NULL,
	[MngrNtcLastSent] [datetime] NULL,
	[SendMgrReport] [bit] NULL,
	[SendGrpReport] [bit] NULL,
	[ReportAdtlAddr] [nvarchar](200) NULL,
	[DaysBtwnReport] [int] NULL,
	[SendHTMLRpt] [bit] NULL,
	[SendProvisionRpts] [bit] NULL,
	[MbrshpLimited] [bit] NULL,
	[MbrshpLimit] [int] NULL,
	[AD3AdminAcntOnly] [bit] NULL,
	[GrpDescription] [nvarchar](2000) NULL,
	[GrpDescriptionAD] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[AGMGID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AGMGroup_Managers]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AGMGroup_Managers](
	[AGMGID] [uniqueidentifier] NOT NULL,
	[AGMMID] [int] NOT NULL,
 CONSTRAINT [AGMGMCID] PRIMARY KEY CLUSTERED 
(
	[AGMGID] ASC,
	[AGMMID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AGMGroup_MemberRequest]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AGMGroup_MemberRequest](
	[AGMGID] [uniqueidentifier] NOT NULL,
	[AGMMRID] [int] NOT NULL,
 CONSTRAINT [AGMMBRREQID] PRIMARY KEY CLUSTERED 
(
	[AGMGID] ASC,
	[AGMMRID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AGMManager]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AGMManager](
	[AGMMID] [int] IDENTITY(1,1) NOT NULL,
	[KerbID] [nvarchar](200) NOT NULL,
	[DisplayName] [nvarchar](400) NULL,
	[EmailAddress] [nvarchar](400) NULL,
	[SendAllGrpsRpt] [bit] NULL,
	[DaysBtwnReport] [int] NULL,
	[RptLastSent] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AGMMID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AGMManagerDisaffiliateRqst]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AGMManagerDisaffiliateRqst](
	[AMDRID] [int] IDENTITY(1,1) NOT NULL,
	[uDept] [nvarchar](25) NULL,
	[KerbID] [nvarchar](200) NULL,
	[DisplayName] [nvarchar](400) NULL,
	[EmailAddress] [nvarchar](400) NULL,
	[SubmittedBy] [nvarchar](200) NULL,
	[SubmittedOn] [datetime] NULL,
	[CompletedOn] [datetime] NULL,
	[DisaffiliateOn] [datetime] NULL,
	[uPending] [bit] NULL,
	[uCancelled] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[AMDRID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AGMManagerSnapShot]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AGMManagerSnapShot](
	[AMSSID] [int] IDENTITY(1,1) NOT NULL,
	[KerbID] [nvarchar](200) NOT NULL,
	[DisplayName] [nvarchar](400) NULL,
	[SSRptDate] [datetime] NULL,
	[EmailAddress] [nvarchar](400) NULL,
PRIMARY KEY CLUSTERED 
(
	[AMSSID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AGMManagerSnapShot_MgmtAssignment]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AGMManagerSnapShot_MgmtAssignment](
	[AMSSMAID] [int] IDENTITY(1,1) NOT NULL,
	[AMSSID] [int] NOT NULL,
	[ADGrpGUID] [nvarchar](200) NULL,
	[ADGrpName] [nvarchar](400) NULL,
	[SSRptDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AMSSMAID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AGMMemberRequest]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AGMMemberRequest](
	[AGMMRID] [int] IDENTITY(1,1) NOT NULL,
	[KerbID] [nvarchar](100) NOT NULL,
	[MRAction] [nvarchar](100) NOT NULL,
	[SubmittedBy] [nvarchar](100) NULL,
	[SubmittedOn] [datetime] NULL,
	[CompletedOn] [datetime] NULL,
	[Pending] [bit] NULL,
	[ADGroupName] [nvarchar](400) NULL,
PRIMARY KEY CLUSTERED 
(
	[AGMMRID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UAGAR_UAODP]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UAGAR_UAODP](
	[UUAARID] [int] NOT NULL,
	[UAODPID] [int] NOT NULL,
 CONSTRAINT [UAGARUAODPID] PRIMARY KEY CLUSTERED 
(
	[UUAARID] ASC,
	[UAODPID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UCDADGroupAppRoles]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UCDADGroupAppRoles](
	[UUAARID] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](200) NULL,
	[RoleGroupDN] [nvarchar](400) NULL,
	[IsDeptRole] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[UUAARID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UCDADObjctDNPartials]    Script Date: 6/11/2021 3:55:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UCDADObjctDNPartials](
	[UAODPID] [int] IDENTITY(1,1) NOT NULL,
	[DNPartial] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[UAODPID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AGMGroup_Managers]  WITH CHECK ADD FOREIGN KEY([AGMGID])
REFERENCES [dbo].[AGMGroup] ([AGMGID])
GO
ALTER TABLE [dbo].[AGMGroup_Managers]  WITH CHECK ADD FOREIGN KEY([AGMMID])
REFERENCES [dbo].[AGMManager] ([AGMMID])
GO
ALTER TABLE [dbo].[AGMGroup_MemberRequest]  WITH CHECK ADD FOREIGN KEY([AGMGID])
REFERENCES [dbo].[AGMGroup] ([AGMGID])
GO
ALTER TABLE [dbo].[AGMGroup_MemberRequest]  WITH CHECK ADD FOREIGN KEY([AGMMRID])
REFERENCES [dbo].[AGMMemberRequest] ([AGMMRID])
GO
ALTER TABLE [dbo].[AGMManagerSnapShot_MgmtAssignment]  WITH CHECK ADD FOREIGN KEY([AMSSID])
REFERENCES [dbo].[AGMManagerSnapShot] ([AMSSID])
GO
ALTER TABLE [dbo].[UAGAR_UAODP]  WITH CHECK ADD FOREIGN KEY([UAODPID])
REFERENCES [dbo].[UCDADObjctDNPartials] ([UAODPID])
GO
ALTER TABLE [dbo].[UAGAR_UAODP]  WITH CHECK ADD FOREIGN KEY([UUAARID])
REFERENCES [dbo].[UCDADGroupAppRoles] ([UUAARID])
GO
