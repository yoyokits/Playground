namespace DeepFakeStudio.Common
{
    #region Enums

    /// <summary>
    /// Defines the ProcessStepId.
    /// </summary>
    public enum ProcessStepId
    {
        ClearWorkspace,
        CutVideo,
        ExtractImagesFullFPS,
        DenoiseDestinationImages,
        DataSourceFaceSetExtractManual,
        DataSourceFaceSetExtract,
        DataSourceViewAlignedResult,
        DataSourceSort,
        DataSourceAddLandmarks,
        DataSourceFaceSetEnhance,
        DataSourceFaceSetMetadataRestore,
        DataSourceFaceSetMetadataSave,
        DataSourceFacesetPack,
        DataSourceFaceSetResize,
        DataSourceFaceSetUnpack,
        DataSourceRecoverOriginalFilename,
        DataDestinationManualFaceSetExtractFix,
        DataDestinationManualFaceSetExtract,
        DataDestinationFaceSetExtract,
        DataDestinationFaceSetReextractDeletedAligned,
        DataDestinationViewAlignedResults,
        DataDestinationViewAlignedDebugResults,
        DataDestinationSort,
        DataDestinationFaceSetPack,
        DataDestinationFaceSetResize,
        DataDestinationFaceSetUnpack,
        DataDestinationFaceSetRecoverOriginalFileName
    }

    #endregion Enums
}