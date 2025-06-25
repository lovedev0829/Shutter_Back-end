-- Create MstPowderCoatColor table
CREATE TABLE [dbo].[MstPowderCoatColor](
	[Id] [uniqueidentifier] NOT NULL DEFAULT (newid()),
	[ColorName] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](200) NULL,
	[IsActive] [bit] NOT NULL DEFAULT (1),
	[IsDelete] [bit] NOT NULL DEFAULT (0),
	[CreatedAt] [datetime] NOT NULL DEFAULT (getdate()),
	[UpdatedAt] [datetime] NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_MstPowderCoatColor] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

-- Add PowderCoatColorId column to TblJob table
ALTER TABLE [dbo].[TblJob]
ADD [PowderCoatColorId] [uniqueidentifier] NULL;

-- Add foreign key constraint
ALTER TABLE [dbo].[TblJob]
ADD CONSTRAINT [FK_TblJob_MstPowderCoatColor] 
FOREIGN KEY([PowderCoatColorId]) REFERENCES [dbo].[MstPowderCoatColor] ([Id]);
