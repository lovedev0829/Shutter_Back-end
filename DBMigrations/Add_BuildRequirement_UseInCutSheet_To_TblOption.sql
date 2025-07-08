-- Add BuildRequirement and UseInCutSheet columns to TblOption table
-- Migration created: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

-- Add the new columns
ALTER TABLE TblOption
ADD BuildRequirement BIT NULL DEFAULT 0,
    UseInCutSheet BIT NULL DEFAULT 0;

-- Update existing records to have default value of 0 (false)
UPDATE TblOption
SET BuildRequirement = 0,
    UseInCutSheet = 0
WHERE BuildRequirement IS NULL 
   OR UseInCutSheet IS NULL;

-- Add comments for documentation
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Flag to indicate if this option is a build requirement', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'TblOption', 
    @level2type = N'COLUMN', @level2name = N'BuildRequirement';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Flag to indicate if this option should be used in cut sheet', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'TblOption', 
    @level2type = N'COLUMN', @level2name = N'UseInCutSheet'; 