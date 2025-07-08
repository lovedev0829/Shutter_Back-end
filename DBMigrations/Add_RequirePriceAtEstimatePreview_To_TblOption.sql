-- Add RequirePriceAtEstimatePreview column to TblOption table
-- Migration created: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

-- Add the new column
ALTER TABLE TblOption
ADD RequirePriceAtEstimatePreview BIT NULL DEFAULT 0;

-- Update existing records to have default value of 0 (false)
UPDATE TblOption
SET RequirePriceAtEstimatePreview = 0
WHERE RequirePriceAtEstimatePreview IS NULL;

-- Add comment for documentation
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Flag to indicate if price is required during estimate preview when AdditionalCost is null', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'TblOption', 
    @level2type = N'COLUMN', @level2name = N'RequirePriceAtEstimatePreview'; 