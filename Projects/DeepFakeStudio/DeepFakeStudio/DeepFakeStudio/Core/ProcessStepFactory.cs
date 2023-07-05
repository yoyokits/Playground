namespace DeepFakeStudio.Core
{
    using System.Collections.Generic;
    using DeepFakeStudio.Models;

    /// <summary>
    /// Defines the <see cref="ProcessStepFactory" />.
    /// </summary>
    internal static class ProcessStepFactory
    {
        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="ProcessStepFactory"/> class.
        /// </summary>
        static ProcessStepFactory()
        {
            ProcessStepFactory.Step01ClearWorkspace = new ProcessStep("Clear workspace", "Delete all files in Workspace folder", "1) clear workspace.bat");

            ProcessStepFactory.Step02ExtractImagesSource = new ProcessStep("Extract images", "Extract images from the video data source", "2) extract images from video data_src.bat");

            ProcessStepFactory.Step03_1CutVideo = new ProcessStep("Cut video", "Cut video (drop video on me)", "3) cut video (drop video on me).bat");
            ProcessStepFactory.Step03_2ExtractImagesFullFPS = new ProcessStep("Extract images Full FPS", "Extract images from video data destination FULL FPS", "3) extract images from video data_dst FULL FPS.bat");
            ProcessStepFactory.Step03_3DenoiseDestinationImages = new ProcessStep("[Optional] Denoise destination images", "[Optional] denoise data_dst images", "3.optional) denoise data_dst images.bat");

            ProcessStepFactory.Step04_01DataSourceFaceSetExtractManual = new ProcessStep("[Manual] Data source face set extract", "Data source face set extract MANUAL", "4) data_src faceset extract MANUAL.bat");
            ProcessStepFactory.Step04_02DataSourceFaceSetExtract = new ProcessStep("Data source face set extract", "Data source face set extract MANUAL", "4) data_src faceset extract MANUAL.bat");
            ProcessStepFactory.Step04_1DataSourceViewAlignedResult = new ProcessStep("Data source view aligned result", "Data source face set extract MANUAL", "4.1) data_src view aligned result.bat");
            ProcessStepFactory.Step04_2_01DataSourceSort = new ProcessStep("Data source sort", "Data source sort", "4.2) data_src sort.bat");
            ProcessStepFactory.Step04_2_02DataSourceAddLandmarks = new ProcessStep("Data source add landmarks", "Data source util add landmarks debug images", "4.2) data_src util add landmarks debug images.bat");
            ProcessStepFactory.Step04_2_03DataSourceFaceSetEnhance = new ProcessStep("Data source faceset enhance", "Data source util faceset enhance", "4.2) data_src util faceset enhance.bat");
            ProcessStepFactory.Step04_2_04DataSourceFaceSetMetadataRestore = new ProcessStep("Data source faceset metadata restore", "Data source util faceset metadata restore", "4.2) data_src util faceset metadata restore.bat");
            ProcessStepFactory.Step04_2_05DataSourceFaceSetMetadataSave = new ProcessStep("Data source faceset metadata save", "Data source faceset metadata save", "4.2) data_src util faceset metadata save.bat");
            ProcessStepFactory.Step04_2_06DataSourceFacesetPack = new ProcessStep("Data source faceset pack", "Data source util faceset pack", "4.2) data_src util faceset pack.bat");
            ProcessStepFactory.Step04_2_07DataSourceFaceSetResize = new ProcessStep("Data source faceset resize", "Data source util faceset resize", "4.2) data_src util faceset resize.bat");
            ProcessStepFactory.Step04_2_08DataSourceFaceSetUnpack = new ProcessStep("Data source faceset unpack", "Data source util faceset unpack", "4.2) data_src util faceset unpack.bat");
            ProcessStepFactory.Step04_2_09DataSourceRecoverOriginalFilename = new ProcessStep("Data source recover original filename", "Data source util recover original filename", "4.2) data_src util recover original filename.bat");

            ProcessStepFactory.Step05_01DataDestinationManualFaceSetExtractFix = new ProcessStep("[Manual] Data destination faceset extract fix", "Data destination faceset extract + manual fix", "5) data_dst faceset extract + manual fix.bat");
            ProcessStepFactory.Step05_02DataDestinationManualFaceSetExtract = new ProcessStep("[Manual] Data destination faceset extract", "Data destination ", "5) data_dst faceset extract MANUAL.bat");
            ProcessStepFactory.Step05_03DataDestinationFaceSetExtract = new ProcessStep("Data destination faceset extract", "Data destination ", "5) data_dst faceset extract.bat");
            ProcessStepFactory.Step05_04DataDestinationFaceSetReextractDeletedAligned = new ProcessStep("Data destination faceset re-extract deleted aligned", "Data destination faceset MANUAL RE-EXTRACT DELETED ALIGNED_DEBUG", "5) data_dst faceset MANUAL RE-EXTRACT DELETED ALIGNED_DEBUG.bat");
            ProcessStepFactory.Step05_11DataDestinationViewAlignedResults = new ProcessStep("Data destination view aligned results", "Data destination view aligned results", "5.1) data_dst view aligned results.bat");
            ProcessStepFactory.Step05_12DataDestinationViewAlignedDebugResults = new ProcessStep("Data destination view aligned_debug results", "Data destination view aligned_debug results", "5.1) data_dst view aligned_debug results.bat");
            ProcessStepFactory.Step05_21DataDestinationSort = new ProcessStep("Data destination sort", "Data destination sort", "5.2) data_dst sort.bat");
            ProcessStepFactory.Step05_22DataDestinationFaceSetPack = new ProcessStep("Data destination faceset pack", "Data destination util faceset pack", "5.2) data_dst util faceset pack.bat");
            ProcessStepFactory.Step05_23DataDestinationFaceSetResize = new ProcessStep("Data destination faceset resize", "Data destination util faceset resize", "5.2) data_dst util faceset resize.bat");
            ProcessStepFactory.Step05_24DataDestinationFaceSetUnpack = new ProcessStep("Data destination faceset unpack", "Data destination util faceset unpack", "5.2) data_dst util faceset unpack.bat");
            ProcessStepFactory.Step05_25DataDestinationFaceSetRecoverOriginalFileName = new ProcessStep("Data destination recover original filename", "Data destination util recover original filename", "5.2) data_dst util recover original filename.bat");

            ProcessStepFactory.Step05_XSegGenericDataDestinationWholeFaceMaskApply = new ProcessStep("Generic data destination whole face mask apply", "Generic data destination whole face mask apply", "5.XSeg Generic) data_dst whole_face mask - apply.bat");
            ProcessStepFactory.Step05_XSegGenericDataSrcWholeFaceMaskApply = new ProcessStep("Generic data source whole face mask apply", "Generic data source whole face mask apply", "5.XSeg Generic) data_src whole_face mask - apply.bat");
            ProcessStepFactory.Step05_XSegDataDestinationMaskEdit = new ProcessStep("Data destination mask Edit", "Data destination mask Edit", "5.XSeg) data_dst mask - edit.bat");
            ProcessStepFactory.Step05_XSegDataDestinationMaskFetch = new ProcessStep("Data destination mask Fetch", "Data destination mask Fetch", "5.XSeg) data_dst mask - fetch.bat");
            ProcessStepFactory.Step05_XSegDataDestinationMaskRemove = new ProcessStep("Data destination mask remove", "Data destination mask remove", "5.XSeg) data_dst mask - remove.bat");
            ProcessStepFactory.Step05_XSegDataDestinationTrainedMaskApply = new ProcessStep("Data destination trained mask apply", "Data destination trained mask apply", "5.XSeg) data_dst trained mask - apply.bat");
            ProcessStepFactory.Step05_XSegDataDestinationTrainedMaskRemove = new ProcessStep("Data destination trained mask remove", "Data destination trained mask remove", "5.XSeg) data_dst trained mask - remove.bat");
            ProcessStepFactory.Step05_XSegDataSourceMaskEdit = new ProcessStep("Data source mask edit", "Data source mask edit", "5.XSeg) data_src mask - edit.bat");
            ProcessStepFactory.Step05_XSegDataSourceMaskFetch = new ProcessStep("Data source mask fetch", "Data source mask fetch", "5.XSeg) data_src mask - fetch.bat");
            ProcessStepFactory.Step05_XSegDataSourceMaskRemove = new ProcessStep("Data source mask remove", "Data source mask remove", "5.XSeg) data_src mask - remove.bat");
            ProcessStepFactory.Step05_XSegDataSourceTrainedMaskApply = new ProcessStep("Data source trained mask apply", "Data source trained mask apply", "5.XSeg) data_src trained mask - apply.bat");
            ProcessStepFactory.Step05_XSegDataSourceTrainedMaskRemove = new ProcessStep("Data Source trained mask remove", "Data Source trained mask remove", "5.XSeg) data_src trained mask - remove.bat");
            ProcessStepFactory.Step05_XSegTrain = new ProcessStep("Train", "Train", "5.XSeg) train.bat");

            ProcessStepFactory.Step06_ExportAmpAsDFM = new ProcessStep("Data destination recover original filename", "Data destination util recover original filename", "6) export AMP as dfm.bat");
            ProcessStepFactory.Step06_ExportSAEHDAsDFM = new ProcessStep("Data destination recover original filename", "Data destination util recover original filename", "6) export SAEHD as dfm.bat");
            ProcessStepFactory.Step06_TrainAMPSourceSource = new ProcessStep("Data destination recover original filename", "Data destination util recover original filename", "6) train AMP SRC-SRC.bat");
            ProcessStepFactory.Step06_TrainAMP = new ProcessStep("Data destination recover original filename", "Data destination util recover original filename", "6) train AMP.bat");
            ProcessStepFactory.Step06_TrainQuick96 = new ProcessStep("Data destination recover original filename", "Data destination util recover original filename", "6) train Quick96.bat");
            ProcessStepFactory.Step06_TrainSAEHD = new ProcessStep("Data destination recover original filename", "Data destination util recover original filename", "6) train SAEHD.bat");

            ProcessStepFactory.Step07_MergeAMP = new ProcessStep("Merge AMP", "Merge AMP", "7) merge AMP.bat");
            ProcessStepFactory.Step07_MergeQuick96 = new ProcessStep("Merge Quick96", "Merge Quick96", "7) merge Quick96.bat");
            ProcessStepFactory.Step07_MergeSAEHD = new ProcessStep("Merge SAEHD", "Merge SAEHD", "7) merge SAEHD.bat");

            ProcessStepFactory.Step08_MergedToAvi = new ProcessStep("Merged to AVI", "Merged to AVI", "8) merged to avi.bat");
            ProcessStepFactory.Step08_MergedToMovLossless = new ProcessStep("Merged to MovLossless", "Merged to MovLossless", "8) merged to mov lossless.bat");
            ProcessStepFactory.Step08_MergedToMp4Lossless = new ProcessStep("Merged to Mp4Lossless", "Merged to Mp4Lossless", "8) merged to mp4 lossless.bat");
            ProcessStepFactory.Step08_MergedToMp4 = new ProcessStep("Merged to Mp4", "Merged to Mp4", "8) merged to mp4.bat");

            ProcessStepFactory.Step10_MiscMakeCPUOnly = new ProcessStep("Make CPU only", "Misc Make CPU only", "10.misc) make CPU only.bat");
            ProcessStepFactory.Step10_MiscStartEBSynth = new ProcessStep("Start EBSynth", "Misc start EBSynth", "10.misc) start EBSynth.bat");

            ProcessStepFactory.RegisterProcessSteps();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the Step05_11DataDestinationViewAlignedResults.
        /// </summary>
        public static ProcessStep Step05_11DataDestinationViewAlignedResults { get; }

        /// <summary>
        /// Gets the Step05_12DataDestinationViewAlignedDebugResults.
        /// </summary>
        public static ProcessStep Step05_12DataDestinationViewAlignedDebugResults { get; }

        /// <summary>
        /// Gets the Step05_21DataDestinationSort.
        /// </summary>
        public static ProcessStep Step05_21DataDestinationSort { get; }

        /// <summary>
        /// Gets the Step05_22DataDestinationFaceSetPack.
        /// </summary>
        public static ProcessStep Step05_22DataDestinationFaceSetPack { get; }

        /// <summary>
        /// Gets the Step05_23DataDestinationFaceSetResize.
        /// </summary>
        public static ProcessStep Step05_23DataDestinationFaceSetResize { get; }

        /// <summary>
        /// Gets the Step05_24DataDestinationFaceSetUnpack.
        /// </summary>
        public static ProcessStep Step05_24DataDestinationFaceSetUnpack { get; }

        /// <summary>
        /// Gets the Step05_25DataDestinationFaceSetRecoverOriginalFileName.
        /// </summary>
        public static ProcessStep Step05_25DataDestinationFaceSetRecoverOriginalFileName { get; }

        /// <summary>
        /// Gets the Step05_XSeg_Generic_data_dst_whole_face_mask_.
        /// </summary>
        public static int Step05_XSeg_Generic_data_dst_whole_face_mask_ { get; }

        /// <summary>
        /// Gets the Step05_XSegDataDestinationMaskEdit.
        /// </summary>
        public static ProcessStep Step05_XSegDataDestinationMaskEdit { get; }

        /// <summary>
        /// Gets the Step05_XSegDataDestinationMaskFetch.
        /// </summary>
        public static ProcessStep Step05_XSegDataDestinationMaskFetch { get; }

        /// <summary>
        /// Gets the Step05_XSegDataDestinationMaskRemove.
        /// </summary>
        public static ProcessStep Step05_XSegDataDestinationMaskRemove { get; }

        /// <summary>
        /// Gets the Step05_XSegDataDestinationTrainedMaskApply.
        /// </summary>
        public static ProcessStep Step05_XSegDataDestinationTrainedMaskApply { get; }

        /// <summary>
        /// Gets the Step05_XSegDataDestinationTrainedMaskRemove.
        /// </summary>
        public static ProcessStep Step05_XSegDataDestinationTrainedMaskRemove { get; }

        /// <summary>
        /// Gets the Step05_XSegDataSourceMaskEdit.
        /// </summary>
        public static ProcessStep Step05_XSegDataSourceMaskEdit { get; }

        /// <summary>
        /// Gets the Step05_XSegDataSourceMaskFetch.
        /// </summary>
        public static ProcessStep Step05_XSegDataSourceMaskFetch { get; }

        /// <summary>
        /// Gets the Step05_XSegDataSourceMaskRemove.
        /// </summary>
        public static ProcessStep Step05_XSegDataSourceMaskRemove { get; }

        /// <summary>
        /// Gets the Step05_XSegDataSourceTrainedMaskApply.
        /// </summary>
        public static ProcessStep Step05_XSegDataSourceTrainedMaskApply { get; }

        /// <summary>
        /// Gets the Step05_XSegDataSourceTrainedMaskRemove.
        /// </summary>
        public static ProcessStep Step05_XSegDataSourceTrainedMaskRemove { get; }

        /// <summary>
        /// Gets the Step05_XSegGenericDataDestinationWholeFaceMaskApply.
        /// </summary>
        public static ProcessStep Step05_XSegGenericDataDestinationWholeFaceMaskApply { get; }

        /// <summary>
        /// Gets the Step05_XSegGenericDataSrcWholeFaceMaskApply.
        /// </summary>
        public static ProcessStep Step05_XSegGenericDataSrcWholeFaceMaskApply { get; }

        /// <summary>
        /// Gets the Step05_XSegTrain.
        /// </summary>
        public static ProcessStep Step05_XSegTrain { get; }

        /// <summary>
        /// Gets the Step06_ExportAmpAsDFM.
        /// </summary>
        public static ProcessStep Step06_ExportAmpAsDFM { get; }

        /// <summary>
        /// Gets the Step06_ExportSAEHDAsDFM.
        /// </summary>
        public static ProcessStep Step06_ExportSAEHDAsDFM { get; }

        /// <summary>
        /// Gets the Step06_TrainAMP.
        /// </summary>
        public static ProcessStep Step06_TrainAMP { get; }

        /// <summary>
        /// Gets the Step06_TrainAMPSourceSource.
        /// </summary>
        public static ProcessStep Step06_TrainAMPSourceSource { get; }

        /// <summary>
        /// Gets the Step06_TrainQuick96.
        /// </summary>
        public static ProcessStep Step06_TrainQuick96 { get; }

        /// <summary>
        /// Gets the Step06_TrainSAEHD.
        /// </summary>
        public static ProcessStep Step06_TrainSAEHD { get; }

        /// <summary>
        /// Gets the Step07_MergeAMP.
        /// </summary>
        public static ProcessStep Step07_MergeAMP { get; }

        /// <summary>
        /// Gets the Step07_MergeQuick96.
        /// </summary>
        public static ProcessStep Step07_MergeQuick96 { get; }

        /// <summary>
        /// Gets the Step07_MergeSAEHD.
        /// </summary>
        public static ProcessStep Step07_MergeSAEHD { get; }

        /// <summary>
        /// Gets the Step08_MergedToAvi.
        /// </summary>
        public static ProcessStep Step08_MergedToAvi { get; }

        /// <summary>
        /// Gets the Step08_MergedToMovLossless.
        /// </summary>
        public static ProcessStep Step08_MergedToMovLossless { get; }

        /// <summary>
        /// Gets the Step08_MergedToMp4.
        /// </summary>
        public static ProcessStep Step08_MergedToMp4 { get; }

        /// <summary>
        /// Gets the Step08_MergedToMp4Lossless.
        /// </summary>
        public static ProcessStep Step08_MergedToMp4Lossless { get; }

        /// <summary>
        /// Gets the Step10_MiscMakeCPUOnly.
        /// </summary>
        public static ProcessStep Step10_MiscMakeCPUOnly { get; }

        /// <summary>
        /// Gets the Step10_MiscStartEBSynth.
        /// </summary>
        public static ProcessStep Step10_MiscStartEBSynth { get; }

        /// <summary>
        /// Gets the StepsList.
        /// </summary>
        public static IList<ProcessStep> StepsList { get; } = new List<ProcessStep>();

        /// <summary>
        /// Gets the Step01ClearWorkspace.
        /// </summary>
        internal static ProcessStep Step01ClearWorkspace { get; }

        /// <summary>
        /// Gets the Step02ExtractImagesSource.
        /// </summary>
        internal static ProcessStep Step02ExtractImagesSource { get; }

        /// <summary>
        /// Gets the Step03_1CutVideo.
        /// </summary>
        internal static ProcessStep Step03_1CutVideo { get; }

        /// <summary>
        /// Gets the Step03_2ExtractImagesFullFPS.
        /// </summary>
        internal static ProcessStep Step03_2ExtractImagesFullFPS { get; }

        /// <summary>
        /// Gets the Step03_3DenoiseDestinationImages.
        /// </summary>
        internal static ProcessStep Step03_3DenoiseDestinationImages { get; }

        /// <summary>
        /// Gets the Step04_01DataSourceFaceSetExtractManual.
        /// </summary>
        internal static ProcessStep Step04_01DataSourceFaceSetExtractManual { get; }

        /// <summary>
        /// Gets the Step04_02DataSourceFaceSetExtract.
        /// </summary>
        internal static ProcessStep Step04_02DataSourceFaceSetExtract { get; }

        /// <summary>
        /// Gets the Step04_1DataSourceViewAlignedResult.
        /// </summary>
        internal static ProcessStep Step04_1DataSourceViewAlignedResult { get; }

        /// <summary>
        /// Gets the Step04_2_01DataSourceSort.
        /// </summary>
        internal static ProcessStep Step04_2_01DataSourceSort { get; }

        /// <summary>
        /// Gets the Step04_2_02DataSourceAddLandmarks.
        /// </summary>
        internal static ProcessStep Step04_2_02DataSourceAddLandmarks { get; }

        /// <summary>
        /// Gets the Step04_2_03DataSourceFaceSetEnhance.
        /// </summary>
        internal static ProcessStep Step04_2_03DataSourceFaceSetEnhance { get; }

        /// <summary>
        /// Gets the Step04_2_04DataSourceFaceSetMetadataRestore.
        /// </summary>
        internal static ProcessStep Step04_2_04DataSourceFaceSetMetadataRestore { get; }

        /// <summary>
        /// Gets the Step04_2_05DataSourceFaceSetMetadataSave.
        /// </summary>
        internal static ProcessStep Step04_2_05DataSourceFaceSetMetadataSave { get; }

        /// <summary>
        /// Gets the Step04_2_06DataSourceFacesetPack.
        /// </summary>
        internal static ProcessStep Step04_2_06DataSourceFacesetPack { get; }

        /// <summary>
        /// Gets the Step04_2_07DataSourceFaceSetResize.
        /// </summary>
        internal static ProcessStep Step04_2_07DataSourceFaceSetResize { get; }

        /// <summary>
        /// Gets the Step04_2_08DataSourceFaceSetUnpack.
        /// </summary>
        internal static ProcessStep Step04_2_08DataSourceFaceSetUnpack { get; }

        /// <summary>
        /// Gets the Step04_2_09DataSourceRecoverOriginalFilename.
        /// </summary>
        internal static ProcessStep Step04_2_09DataSourceRecoverOriginalFilename { get; }

        /// <summary>
        /// Gets the Step05_01DataDestinationManualFaceSetExtractFix.
        /// </summary>
        internal static ProcessStep Step05_01DataDestinationManualFaceSetExtractFix { get; }

        /// <summary>
        /// Gets the Step05_02DataDestinationManualFaceSetExtract.
        /// </summary>
        internal static ProcessStep Step05_02DataDestinationManualFaceSetExtract { get; }

        /// <summary>
        /// Gets the Step05_03DataDestinationFaceSetExtract.
        /// </summary>
        internal static ProcessStep Step05_03DataDestinationFaceSetExtract { get; }

        /// <summary>
        /// Gets the Step05_04DataDestinationFaceSetReextractDeletedAligned.
        /// </summary>
        internal static ProcessStep Step05_04DataDestinationFaceSetReextractDeletedAligned { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// The CreateProcessSteps.
        /// </summary>
        /// <returns>The <see cref="IList{ProcessStep}"/>.</returns>
        public static IList<ProcessStep> CreateProcessSteps()
        {
            var processSteps = new List<ProcessStep>(ProcessStepFactory.StepsList.Count);
            foreach (var step in ProcessStepFactory.StepsList)
            {
                processSteps.Add(new ProcessStep(step));
            }

            return processSteps;
        }

        /// <summary>
        /// The RegisterProcessSteps.
        /// </summary>
        private static void RegisterProcessSteps()
        {
            ProcessStepFactory.StepsList.Add(Step01ClearWorkspace);

            ProcessStepFactory.StepsList.Add(Step02ExtractImagesSource);

            ProcessStepFactory.StepsList.Add(Step03_1CutVideo);
            ProcessStepFactory.StepsList.Add(Step03_2ExtractImagesFullFPS);
            ProcessStepFactory.StepsList.Add(Step03_3DenoiseDestinationImages);

            ProcessStepFactory.StepsList.Add(Step04_01DataSourceFaceSetExtractManual);
            ProcessStepFactory.StepsList.Add(Step04_02DataSourceFaceSetExtract);
            ProcessStepFactory.StepsList.Add(Step04_1DataSourceViewAlignedResult);
            ProcessStepFactory.StepsList.Add(Step04_2_01DataSourceSort);
            ProcessStepFactory.StepsList.Add(Step04_2_02DataSourceAddLandmarks);
            ProcessStepFactory.StepsList.Add(Step04_2_03DataSourceFaceSetEnhance);
            ProcessStepFactory.StepsList.Add(Step04_2_04DataSourceFaceSetMetadataRestore);
            ProcessStepFactory.StepsList.Add(Step04_2_05DataSourceFaceSetMetadataSave);
            ProcessStepFactory.StepsList.Add(Step04_2_06DataSourceFacesetPack);
            ProcessStepFactory.StepsList.Add(Step04_2_07DataSourceFaceSetResize);
            ProcessStepFactory.StepsList.Add(Step04_2_08DataSourceFaceSetUnpack);
            ProcessStepFactory.StepsList.Add(Step04_2_09DataSourceRecoverOriginalFilename);

            ProcessStepFactory.StepsList.Add(Step05_01DataDestinationManualFaceSetExtractFix);
            ProcessStepFactory.StepsList.Add(Step05_02DataDestinationManualFaceSetExtract);
            ProcessStepFactory.StepsList.Add(Step05_03DataDestinationFaceSetExtract);
            ProcessStepFactory.StepsList.Add(Step05_04DataDestinationFaceSetReextractDeletedAligned);
            ProcessStepFactory.StepsList.Add(Step05_11DataDestinationViewAlignedResults);
            ProcessStepFactory.StepsList.Add(Step05_12DataDestinationViewAlignedDebugResults);
            ProcessStepFactory.StepsList.Add(Step05_21DataDestinationSort);
            ProcessStepFactory.StepsList.Add(Step05_22DataDestinationFaceSetPack);
            ProcessStepFactory.StepsList.Add(Step05_23DataDestinationFaceSetResize);
            ProcessStepFactory.StepsList.Add(Step05_24DataDestinationFaceSetUnpack);
            ProcessStepFactory.StepsList.Add(Step05_25DataDestinationFaceSetRecoverOriginalFileName);

            ProcessStepFactory.StepsList.Add(Step05_XSegGenericDataDestinationWholeFaceMaskApply);
            ProcessStepFactory.StepsList.Add(Step05_XSegGenericDataSrcWholeFaceMaskApply);
            ProcessStepFactory.StepsList.Add(Step05_XSegDataDestinationMaskEdit);
            ProcessStepFactory.StepsList.Add(Step05_XSegDataDestinationMaskFetch);
            ProcessStepFactory.StepsList.Add(Step05_XSegDataDestinationMaskRemove);
            ProcessStepFactory.StepsList.Add(Step05_XSegDataDestinationTrainedMaskApply);
            ProcessStepFactory.StepsList.Add(Step05_XSegDataDestinationTrainedMaskRemove);
            ProcessStepFactory.StepsList.Add(Step05_XSegDataSourceMaskEdit);
            ProcessStepFactory.StepsList.Add(Step05_XSegDataSourceMaskFetch);
            ProcessStepFactory.StepsList.Add(Step05_XSegDataSourceMaskRemove);
            ProcessStepFactory.StepsList.Add(Step05_XSegDataSourceTrainedMaskApply);
            ProcessStepFactory.StepsList.Add(Step05_XSegDataSourceTrainedMaskRemove);
            ProcessStepFactory.StepsList.Add(Step05_XSegTrain);

            ProcessStepFactory.StepsList.Add(Step06_ExportAmpAsDFM);
            ProcessStepFactory.StepsList.Add(Step06_ExportSAEHDAsDFM);
            ProcessStepFactory.StepsList.Add(Step06_TrainAMPSourceSource);
            ProcessStepFactory.StepsList.Add(Step06_TrainAMP);
            ProcessStepFactory.StepsList.Add(Step06_TrainQuick96);
            ProcessStepFactory.StepsList.Add(Step06_TrainSAEHD);

            ProcessStepFactory.StepsList.Add(Step07_MergeAMP);
            ProcessStepFactory.StepsList.Add(Step07_MergeQuick96);
            ProcessStepFactory.StepsList.Add(Step07_MergeSAEHD);

            ProcessStepFactory.StepsList.Add(Step08_MergedToAvi);
            ProcessStepFactory.StepsList.Add(Step08_MergedToMovLossless);
            ProcessStepFactory.StepsList.Add(Step08_MergedToMp4Lossless);
            ProcessStepFactory.StepsList.Add(Step08_MergedToMp4);

            ProcessStepFactory.StepsList.Add(Step10_MiscMakeCPUOnly);
            ProcessStepFactory.StepsList.Add(Step10_MiscStartEBSynth);
        }

        #endregion Methods
    }
}