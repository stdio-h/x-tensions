﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;

namespace XTensions
{
    public static class HelperMethods
    {
        private static readonly int _volumeNameBufferLength = 256 * 2;
        private static readonly int _sectorContentsBufferLength = 512 * 2;
        private static readonly int _itemTypeDescriptionBufferLength = 1024 * 2;
        private static readonly int _casePropertiesLength = 1024 * 2;
        private static readonly int _eventObjectPropertiesLength = 128 * 2;

        /// <summary>
        /// Retrieve the size of the provided volume or file.  A helper method for 
        /// XWF_GetSize().
        /// </summary>
        /// <param name="volumeOrItem">A pointer to a volume or item.</param>
        /// <param name="sizeType">The type (XWFGetSizeType) of size to retrieve.
        /// </param>
        /// <returns>Returns the size of the volume or file.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static long GetSize(IntPtr volumeOrItem, ItemSizeType sizeType
            = ItemSizeType.PhysicalSize)
        {
            // Fail if a volume or item pointer weren't provided.
            if (volumeOrItem == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            return ImportedMethods.XWF_GetSize(volumeOrItem, sizeType);
        }

        /// <summary>
        /// Retrieves the UTF-16 name of the provided volume, using 255 characters at 
        /// most. A helper method for XWF_GetVolumeName().
        /// </summary>
        /// <param name="volume">A pointer to a volume.</param>
        /// <param name="volumeNameType">The volume name type (XWFVolumeNameType) to
        /// return.</param>
        /// <returns>Returns the volume name in the type specified.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static string GetVolumeName(IntPtr volume, VolumeNameType volumeNameType 
            = VolumeNameType.Type1)
        {
            // Fail if a volume pointer wasn't provided.
            if (volume == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            // Allocate a buffer to receive the volume name, call the API function and
            // get volume name, and clean up.
            IntPtr Buffer = Marshal.AllocHGlobal(_volumeNameBufferLength);
            ImportedMethods.XWF_GetVolumeName(volume, Buffer, volumeNameType);
            string VolumeName = Marshal.PtrToStringUni(Buffer);
            Marshal.FreeHGlobal(Buffer);

            return VolumeName;
        }

        /// <summary>
        /// Retrieves various information about a given volume. A helper method for 
        /// XWF_GetVolumeInformation().
        /// </summary>
        /// <param name="volume">A pointer to a volume.</param>
        /// <returns>Returns the volume information in a VolumeInformation struct.
        /// </returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static VolumeInformation GetVolumeInformation(IntPtr volume)
        {
            // Fail if a volume pointer wasn't provided.
            if (volume == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            VolumeInformation info = new VolumeInformation();

            // Call the API function, getting the volume's information.
            ImportedMethods.XWF_GetVolumeInformation(volume, out info.FileSystem
                , out info.BytesPerSector, out info.SectorsPerCluster
                , out info.ClusterCount, out info.FirstClusterSectorNumber);

            return info;
        }

        /// <summary>
        /// Retrieves the boundaries of the currently selected block, if any. A helper 
        /// method for XWF_GetBlock().
        /// </summary>
        /// <param name="volume">A pointer to a volume.</param>
        /// <returns>Returns the block boundaries in a BlockBoundaries struct.  These 
        /// boundaries will be 0 and -1 respectively if no block is selected.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static BlockBoundaries GetBlock(IntPtr volume)
        {
            // Fail if a volume pointer wasn't provided.
            if (volume == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            BlockBoundaries Boundaries = new BlockBoundaries();

            // Call the API function, getting the block's boundaries.
            bool Result = ImportedMethods.XWF_GetBlock(volume
                , out Boundaries.StartOffset, out Boundaries.EndOffset);

            return Boundaries;
        }

        /// <summary>
        /// Set the boundaries (provided as a BlockBoundaries struct) of a new block. A
        /// helper method for XWF_SetBlock().
        /// </summary>
        /// <param name="volume">A pointer to a volume.</param>
        /// <param name="boundaries">A BlockBoundaries struct indicating the starting
        /// and ending offsets of the new block.</param>
        /// <returns>Returns True if successful and False if the provided boundaries 
        /// exceed the size of the volume.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static bool SetBlock(IntPtr volume, BlockBoundaries boundaries)
        {
            // Fail if a volume pointer wasn't provided.
            if (volume == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            // Return results of API call to set the block.
            return ImportedMethods.XWF_SetBlock(volume, boundaries.StartOffset
                , boundaries.EndOffset);
        }

        /// <summary>
        /// Clear an existing block.
        /// </summary>
        /// <param name="volume">A pointer to a volume.</param>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Need thorough testing.</remarks>
        public static void ClearBlock(IntPtr volume)
        {
            // Fail if a volume pointer wasn't provided.
            if (volume == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            // Return results of API call to clear the block.
            ImportedMethods.XWF_SetBlock(volume, 0, -1);
        }

        /// <summary>
        /// Retrieves information about a provided sector. A helper method for 
        /// XWF_GetSectorContents().
        /// </summary>
        /// <param name="volume">A pointer to a volume.</param>
        /// <param name="sectorNumber">The sector number.</param>
        /// <returns>Returns the sector information in a SectorInformation struct.
        /// </returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static SectorInformation GetSectorContents(IntPtr volume
            , long sectorNumber)
        {
            // Fail if a volume pointer wasn't provided.
            if (volume == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            SectorInformation SectorInformation;

            // Allocate a buffer to receive the sector contents description.
            IntPtr Buffer = Marshal.AllocHGlobal(_sectorContentsBufferLength);

            // Get the results from the sector information API call and clean up.
            SectorInformation.IsAllocated = ImportedMethods.XWF_GetSectorContents(volume
                , sectorNumber, Buffer, out SectorInformation.OwnerItemID);

            // The state of the buffer determines whether there is a description.
            SectorInformation.Description = (Buffer != IntPtr.Zero)
                ? Marshal.PtrToStringUni(Buffer) : null;
            Marshal.FreeHGlobal(Buffer);

            return SectorInformation;
        }

        /// <summary>
        /// Opens the provided item for reading. Available in v16.5 and later. A helper
        /// method for XWF_OpenItem().
        /// </summary>
        /// <param name="volume">A pointer to a volume.</param>
        /// <param name="itemId">The item's Id.</param>
        /// <param name="options">Options for opening, using ItemOpenModes flag.</param>
        /// <returns>Returns a handle to the open item, or IntPtr.Zero if unsuccessful
        /// </returns>
        /// <remarks>Version 1.0 coding complete.
        /// Todo: Investigate exceptions better.</remarks>
        public static IntPtr OpenItem(IntPtr volume, long itemId, ItemOpenModes options 
            = ItemOpenModes.LogicalContents)
        {
            // Fail if a volume pointer wasn't provided.
            if (volume == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            try
            {
                return ImportedMethods.XWF_OpenItem(volume, itemId, options);
            }
            catch (System.AccessViolationException e)
            {
                OutputMessage("Exception: " + e);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Closes a volume or an item that was opened with the OpenItem method. 
        /// Available in v16.5 and later. A helper method for XWF_Close().
        /// </summary>
        /// <param name="volumeOrItem">An open volume or item.</param>
        /// <returns>Returns true if a successfull, otherwise false.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// Todo: Can we use volume/item interchangably in these circumstances?</remarks>
        public static bool CloseItem(IntPtr volumeOrItem)
        {
            // Fail if a volume or item pointer weren't provided.
            if (volumeOrItem == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            ImportedMethods.XWF_Close(volumeOrItem);
            return true;
        }

        /// <summary>
        /// Reads the specified number of bytes from a specified position in a specified 
        /// item. A helper method for XWF_Read().
        /// </summary>
        /// <param name="volumeOrItem">A pointer to a volume or item.</param>
        /// <param name="offset">The offset to read from.</param>
        /// <param name="numberOfBytesToRead">The number of bytes to read.</param>
        /// <returns>Returns a byte array of the bytes read.</returns>
        /// <remarks>Version 1.0 coding complete. 
        /// - Todo: Does XWF_Read really need to use a DWORD (uint) for number of bytes 
        /// to read?</remarks>
        public static byte[] Read(IntPtr volumeOrItem, long offset = 0
            , int numberOfBytesToRead = 0)
        {
            // Fail if a volume or item pointer weren't provided.
            if (volumeOrItem == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            // Read the full file from the provided offset if the provided number of 
            // bytes to read is 0.
            if (numberOfBytesToRead == 0)
            {
                numberOfBytesToRead = (int)(GetSize(volumeOrItem
                    , ItemSizeType.PhysicalSize) - offset);
            }

            // Initialize and create a pointer to the buffer.
            IntPtr Buffer = Marshal.AllocHGlobal(numberOfBytesToRead);

            // Call XWF_Read to read the item's data into the buffer.
            ImportedMethods.XWF_Read(volumeOrItem, offset, Buffer
                , (uint)numberOfBytesToRead);

            // Copy the buffer contents into a byte array and cleanup the buffer.
            byte[] contents = new byte[numberOfBytesToRead];
            Marshal.Copy(Buffer, contents, 0, numberOfBytesToRead);
            Marshal.FreeHGlobal(Buffer);

            return contents;
        }

        /// <summary>
        /// Retrieves information about the current case. A helper method for 
        /// XWF_GetCaseProps().
        /// </summary>
        /// <returns>Returns a CaseProperties structure.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Need to handle when -1 is returned from API call; indicating that no 
        /// case is loaded.</remarks>
        public static CaseProperties GetCaseProperties()
        {
            CaseProperties Properties = new CaseProperties();

            // Read the title.
            IntPtr Buffer = Marshal.AllocHGlobal(_casePropertiesLength);
            ImportedMethods.XWF_GetCaseProp(IntPtr.Zero, (int)CasePropertyType.CaseTitle
                , Buffer, _casePropertiesLength);
            Properties.CaseTitle = Marshal.PtrToStringUni(Buffer);
            Marshal.FreeHGlobal(Buffer);

            // Read the examiner.
            Buffer = Marshal.AllocHGlobal(_casePropertiesLength);
            ImportedMethods.XWF_GetCaseProp(IntPtr.Zero
                , (int)CasePropertyType.CaseExaminer, Buffer, _casePropertiesLength);
            Properties.CaseExaminer = Marshal.PtrToStringUni(Buffer);
            Marshal.FreeHGlobal(Buffer);

            // Read the case file path.
            Buffer = Marshal.AllocHGlobal(_casePropertiesLength);
            ImportedMethods.XWF_GetCaseProp(IntPtr.Zero
                , (int)CasePropertyType.CaseFilePath, Buffer, _casePropertiesLength);
            Properties.CaseFilePath = Marshal.PtrToStringUni(Buffer);
            Marshal.FreeHGlobal(Buffer);

            // Read the case directory.
            Buffer = Marshal.AllocHGlobal(_casePropertiesLength);
            ImportedMethods.XWF_GetCaseProp(IntPtr.Zero
                , (int)CasePropertyType.CaseDirectory, Buffer, _casePropertiesLength);
            Properties.CaseDirectory = Marshal.PtrToStringUni(Buffer);
            Marshal.FreeHGlobal(Buffer);

            return Properties;
        }

        /// <summary>
        /// Retrieves a handle to the first evidence object in the case. In conjunction
        /// with XWF_GetNextEvObj this function allows to enumerate all evidence objects
        /// of the case. Available from v17.6. A helper method for XWF_GetFirstEvObj().
        /// </summary>
        /// <returns>Returns a pointer to the first evidence objector, or NULL if the 
        /// active case has no evidence objects or (in releases from June 2016) if no 
        /// case is active.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static IntPtr GetFirstEvidenceObject()
        {
            return ImportedMethods.XWF_GetFirstEvObj(IntPtr.Zero);
        }

        /// <summary>
        /// Given a pointer to the previous evidence object, retrieves the next evidence
        /// object in the chain. Available from v17.6. A helper method for 
        /// XWF_GetNextEvObj().
        /// </summary>
        /// <param name="previousEvidence">Previous evidence object.</param>
        /// <returns>Returns a pointer to the next evidence object.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static IntPtr GetNextEvidenceObject(IntPtr previousEvidence)
        {
            // Handle case where zero pointer is provided.
            if (previousEvidence == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            return ImportedMethods.XWF_GetNextEvObj(previousEvidence, IntPtr.Zero);
        }

        /*
        /// <summary>
        /// NOT CURRENTLY IMPLEMENTED. Removes the specified evidence object from the 
        /// case. 
        /// </summary>
        /// <param name="EvidenceObject">Evidence object to be deleted.</param>
        /// <returns></returns>
        public static IntPtr XWF_DeleteEvObj(IntPtr EvidenceObject)
        {
            return IntPtr.Zero;
        }
        */

        /// <summary>
        /// Creates one or more evidence objects from one source (which can be a medium,
        /// disk/volume image, memory dump, or a directory/path). A case must already be 
        /// loaded. If more than 1 evidence object is created (for example for a physical 
        /// disk that contains partitions, which count as evidence objects themselves), 
        /// use XWF_GetNextEvObj to find them. Available in v16.5 and later. A helper 
        /// method for XWF_CreateEvObj().
        /// </summary>
        /// <param name="evidenceType">The evidence object type.</param>
        /// <param name="objectPath">Path in case of a file or directory, otherwise NULL.
        /// </param>
        /// <returns>Returns the first evidence object created, or NULL in case of an
        /// error.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Not sure a marshalled type is needed in the parameters.</remarks>
        public static IntPtr CreateEvidenceObject(EvidenceObjectType evidenceType
            , [MarshalAs(UnmanagedType.LPWStr)] string objectPath = null)
        {
            // Make sure a path was provided if one is expected.
            if ((evidenceType == EvidenceObjectType.DiskImage
                || evidenceType == EvidenceObjectType.MemoryDump
                || evidenceType == EvidenceObjectType.Directory
                || evidenceType == EvidenceObjectType.File) && objectPath == null)
            {
                return IntPtr.Zero;
            }

            EvidenceObjectCategory EvidenceType;

            // Determine the type based on the disk ID provided.
            switch (evidenceType)
            {
                case EvidenceObjectType.DiskImage:
                    EvidenceType = EvidenceObjectCategory.Image;
                    evidenceType = EvidenceObjectType.FileBased;
                    break;
                case EvidenceObjectType.MemoryDump:
                    EvidenceType = EvidenceObjectCategory.MemoryDump;
                    evidenceType = EvidenceObjectType.FileBased;
                    break;
                case EvidenceObjectType.Directory:
                    EvidenceType = EvidenceObjectCategory.Directory;
                    evidenceType = EvidenceObjectType.FileBased;
                    break;
                case EvidenceObjectType.File:
                    EvidenceType = EvidenceObjectCategory.File;
                    evidenceType = EvidenceObjectType.FileBased;
                    break;
                default:
                    EvidenceType = EvidenceObjectCategory.Disk;
                    break;
            }

            return ImportedMethods.XWF_CreateEvObj(EvidenceType, evidenceType, objectPath
                , IntPtr.Zero);
        }

        /// <summary>
        /// If not currently open, opens the specified evidence object in a data window
        /// (and at the operating system level opens the corresponding disk or image
        /// file), interprets the image file (if the evidence object is an image), loads
        /// or takes the volume snapshot and returns a handle to the volume that the
        /// evidence object represents. Use this function if you wish to read data from 
        /// the volume or process the volume snapshot. Potentially time-consuming. 
        /// Available from v17.6. Options must be EvidenceOpenOptions.None in v18.0 and 
        /// older. A helper method for XWF_OpenEvObj().
        /// </summary>
        /// <param name="evidence">A pointer to the evidence object.</param>
        /// <param name="options">EvidenceOpenOptions open options.</param>
        /// <returns>Returns a handle to the volume that the evidence object represents 
        /// or returns 0 if unsuccessful.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static IntPtr OpenEvidenceObject(IntPtr evidence
            , EvidenceOpenOptions options)
        {
            return ImportedMethods.XWF_OpenEvObj(evidence, options);
        }

        /// <summary>
        /// Closes the specified evidence object if it is open currently and unloads the
        /// volume snapshot, otherwise does nothing. Available from v17.6. A helper 
        /// method for XWF_CloseEvObj().
        /// </summary>
        /// <param name="evidence">The evidence object to close.</param>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static void CloseEvidenceObject(IntPtr evidence)
        {
            ImportedMethods.XWF_CloseEvObj(evidence);
        }

        /// <summary>
        /// Retrieves information about the specified evidence object. Does not require
        /// that the evidence object be open. Available from v17.6. A helper method for
        /// XWF_GetEvObjProp().
        /// </summary>
        /// <param name="evidence">A pointer to the evidence object.</param>
        /// <returns>Returns a EvidenceObjectProperites struct.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static EvidenceObjectProperties GetEvidenceObjectProperties(
            IntPtr evidence)
        {
            // Fail if an evidence pointer wasn't provided.
            if (evidence == IntPtr.Zero) throw new ArgumentException(
                "Zero evidence pointer provided.");

            EvidenceObjectProperties Properties = new EvidenceObjectProperties();

            // Get the object number.
            Properties.objectNumber = ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.EvidenceObjectNumber, IntPtr.Zero);

            // Get the object ID.
            Properties.objectID = ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.EvidenceObjectID, IntPtr.Zero);

            // Get the parent object ID.
            Properties.parentObjectID = ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.ParentEvidenceObjectID, IntPtr.Zero);

            // Get the title.
            long tmpPtr = ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.Title, IntPtr.Zero);
            Properties.title = Marshal.PtrToStringUni((IntPtr)tmpPtr);

            // Initialize the buffer for later use.
            int bufferSize = _eventObjectPropertiesLength;

            // Get the extended title.
            IntPtr bufferPtr = Marshal.AllocHGlobal(bufferSize);
            bufferPtr = Marshal.AllocHGlobal(bufferSize);
            ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.ExtendedTitle, bufferPtr);
            Properties.extendedTitle = Marshal.PtrToStringUni(bufferPtr);
            Marshal.FreeHGlobal(bufferPtr);

            // Get the abbreviated title.
            bufferPtr = Marshal.AllocHGlobal(bufferSize);
            ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.AbbreviatedTitle, bufferPtr);
            Properties.abbreviatedTitle = Marshal.PtrToStringUni(bufferPtr);
            Marshal.FreeHGlobal(bufferPtr);

            // Get the internal name.
            tmpPtr = ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.InternalName, IntPtr.Zero);
            Properties.internalName = Marshal.PtrToStringUni((IntPtr)tmpPtr);

            // Get the description.
            tmpPtr = ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.Description, IntPtr.Zero);
            Properties.description = Marshal.PtrToStringUni((IntPtr)tmpPtr);

            // Get the examiner comments.
            tmpPtr = ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.ExaminerComments, IntPtr.Zero);
            Properties.examinerComments = Marshal.PtrToStringUni((IntPtr)tmpPtr);

            // Get the internally used directory.
            bufferPtr = Marshal.AllocHGlobal(bufferSize);
            ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.InternallyUsedDirectory, bufferPtr);
            Properties.internallyUsedDirectory = Marshal.PtrToStringUni(bufferPtr);
            Marshal.FreeHGlobal(bufferPtr);

            // Get the output directory.
            bufferPtr = Marshal.AllocHGlobal(bufferSize);
            ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.OutputDirectory, bufferPtr);
            Properties.outputDirectory = Marshal.PtrToStringUni(bufferPtr);
            Marshal.FreeHGlobal(bufferPtr);

            // Get the size in bytes.
            Properties.SizeInBytes = ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.SizeInBytes, IntPtr.Zero);

            // Get the volume snapshot file count.
            Properties.VolumeSnapshotFileCount = ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.VolumeSnapshotFileCount, IntPtr.Zero);

            // Get the flags.
            Properties.Flags = (EvidenceProperties)ImportedMethods.XWF_GetEvObjProp(
                evidence, EvidencePropertyType.Flags, IntPtr.Zero);

            // Get the file system identifier.
            Properties.FileSystemIdentifier
                = (VolumeFileSystem)ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.FileSystemIdentifier, IntPtr.Zero);

            // Get the hash type.
            Properties.HashType = (HashType)ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.HashType, IntPtr.Zero);

            // Get the hash value.
            if (Properties.HashType == HashType.Undefined)
            {
                Properties.HashValue = null;
            }
            else
            {
                bufferPtr = Marshal.AllocHGlobal(bufferSize);
                int hashSize = (int)ImportedMethods.XWF_GetEvObjProp(evidence
                    , EvidencePropertyType.HashValue, bufferPtr);
                Byte[] hash1 = new Byte[hashSize];
                Marshal.Copy(bufferPtr, hash1, 0, hashSize);
                Properties.HashValue = hash1;
                Marshal.FreeHGlobal(bufferPtr);
            }

            // Get the creation time.
            Properties.CreationTime = DateTime.FromFileTime(ImportedMethods.XWF_GetEvObjProp(
                evidence, EvidencePropertyType.CreationTime, IntPtr.Zero));

            // Get the modification time.
            Properties.ModificationTime = DateTime.FromFileTime(
                ImportedMethods.XWF_GetEvObjProp(evidence
                    , EvidencePropertyType.ModificationTime, IntPtr.Zero));

            // Get the hash 2 type.
            Properties.HashType2 = (HashType)ImportedMethods.XWF_GetEvObjProp(evidence
                , EvidencePropertyType.HashType2, IntPtr.Zero);

            // Get the hash 2 value.
            if (Properties.HashType2 == HashType.Undefined)
            {
                Properties.HashValue2 = null;
            }
            else
            {
                bufferPtr = Marshal.AllocHGlobal(bufferSize);
                int hashSize = (int)ImportedMethods.XWF_GetEvObjProp(evidence
                    , EvidencePropertyType.HashValue2, bufferPtr);
                Byte[] hash2 = new Byte[hashSize];
                Marshal.Copy(bufferPtr, hash2, 0, hashSize);
                Properties.HashValue2 = hash2;
                Marshal.FreeHGlobal(bufferPtr);
            }

            return Properties;
        }

        /// <summary>
        /// Retrieves a handle to the evidence object with the specified unique ID. The 
        /// unique ID of an evidence object remains the same after closing and re-opening 
        /// a case, whereas the handle will likely change. The evidence object number may 
        /// also change. That happens if the user re-orders the evidence objects in the 
        /// case. The unique ID, however, is guaranteed to never change and also 
        /// guaranteed to be unique within the case (actually likely unique even across 
        /// all the cases that the user will even deal with) and can be used to reliably 
        /// recognize a known evidence object. Available from v18.7. A helper method for 
        /// XWF_GetEvObj().
        /// </summary>
        /// <param name="evidenceObjectId"></param>
        /// <returns>Returns a pointer to the evidence object corresponding to the 
        /// specified evidence object Id or NULL if not found.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Should I change name to GetEvidenceObjectById?</remarks>
        private static IntPtr GetEvidenceObject(uint evidenceObjectId)
        {
            return ImportedMethods.XWF_GetEvObj(evidenceObjectId);
        }

        /// <summary>
        /// Gets the name of a report table, null if none, or the maximum number of
        /// report table names if reportTableId is set to -1. Valid report table IDs 
        /// range from 0 to (maximum number: -1).  Available in v17.7 and later. A helper
        /// method for XWF_GetReportTableInfo().
        /// </summary>
        /// <param name="reportTableId">An existing report table ID or -1 to get the 
        /// maximum number of report tables.</param>
        /// <param name="informationOptions">ReportTableInformationOptions options.
        /// Should be ReportTableInformationOptions.None before v18.1.</param>
        /// <returns>Returns the name of a given report table or null if none.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Right now catching exceptions; need to figure out what's happening.
        /// - Todo: Need to test what happens when -1 if supplied.</remarks>
        public static string GetReportTableInformation(int reportTableId
            , ReportTableInformationOptions informationOptions)
        {
            try
            {
                IntPtr Buffer = ImportedMethods.XWF_GetReportTableInfo(
                    IntPtr.Zero, reportTableId, informationOptions);
                string ReportName = Marshal.PtrToStringUni(Buffer);
                Marshal.FreeHGlobal(Buffer);

                return ReportName;
            }
            catch (System.AccessViolationException e)
            {
                OutputMessage("Exception: " + e);
                return null;
            }
        }

        /// <summary>
        /// Returns a pointer to an internal list that describes all report table
        /// associations of the specified evidence object, or NULL if unsuccessful (for
        /// example if not available any more in a future version). Scanning this list
        /// is a much quicker way to find out which items are associated with a given
        /// report table than calling GetReportTableAssocs for all items in a volume
        /// snapshot, especially if the snapshot is huge. The list consists of 16-bit
        /// report table ID and 32-bit item ID pairs repeatedly, stored back to back. A
        /// helper method for XWF_GetEvObjReportTableAssocs().
        /// </summary>
        /// <param name="evidence">A pointer to the evidence object.</param>
        /// <param name="options">Optional. Return list flags.</param>
        /// <returns></returns>
        /// <remarks>Version 0.5 coding complete.
        /// - Todo: Need to test the output and build an array of items returned.
        /// - Todo: Need to figure out what the flags are used for.
        /// </remarks>
        public static IntPtr GetEvidenceObjectReportTableAssociations(IntPtr evidence
            , uint options = 0x01)
        {
            IntPtr Value;
            IntPtr associationList = ImportedMethods.XWF_GetEvObjReportTableAssocs(
                evidence, options, out Value);

            return Value;
        }

        /// <summary>
        /// Defines to which volume's volume snapshot subsequent calls of the below
        /// functions apply should you wish to change that. A helper method for 
        /// XWF_SelectVolumeSnapshot().
        /// </summary>
        /// <param name="volume">A pointer to the specified volume.</param>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static void SelectVolumeSnapshot(IntPtr volume)
        {
            // Fail if an evidence pointer wasn't provided.
            if (volume == IntPtr.Zero) throw new ArgumentException(
                "Zero volume pointer provided.");

            ImportedMethods.XWF_SelectVolumeSnapshot(volume);
        }

        /// <summary>
        /// Retrieves information about the current volume snapshot. Available in v17.4
        /// and later. A helper method for XWF_GetVSProp().
        /// </summary>
        /// <param name="propertyType">Property type.</param>
        /// <param name="specialItemType">Optional. Special item type. Only required
        /// when propertyType is SpecialItemID.</param>
        /// <returns>Returns the property requested.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Not sure we need this since we have another method below to get all 
        /// properties.</remarks>
        public static long GetVolumeSnapshotProperties(
            VolumeSnapshotPropertyType propertyType, SpecialItemType specialItemType = SpecialItemType.Ununsed)
        {
            return ImportedMethods.XWF_GetVSProp(propertyType, specialItemType);
        }

        /// <summary>
        /// Retrieves all available properties of the current volume snapshot.  Available
        /// in v17.4 and above. A helper method for XWF_GetVSProps().
        /// </summary>
        /// <returns>Returns the properties in a VolumeSnapshotProperties structure.
        /// </returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static VolumeSnapshotProperties GetVolumeSnapshotProperties()
        {
            VolumeSnapshotProperties Properties = new VolumeSnapshotProperties();

            // Get the root directory.
            Properties.rootDirectory = ImportedMethods.XWF_GetVSProp(
                VolumeSnapshotPropertyType.SpecialItemID
                , SpecialItemType.RootDirectory);
            // Get the "Path Unknown" directory.
            Properties.pathUnknownDirectory = ImportedMethods.XWF_GetVSProp(
                VolumeSnapshotPropertyType.SpecialItemID
                , SpecialItemType.PathUnknownDirectory);
            // Get the "Carved Files" directory.
            Properties.carvedFilesDirectory = ImportedMethods.XWF_GetVSProp(
                VolumeSnapshotPropertyType.SpecialItemID
                , SpecialItemType.CarvedFilesDirectory);
            // Get the free space file.
            Properties.freeSpaceFile = ImportedMethods.XWF_GetVSProp(
                VolumeSnapshotPropertyType.SpecialItemID
                , SpecialItemType.FreeSpaceFile);
            // Get the "System Volume Information" directory.
            Properties.systemVolumeInformationDirectory = ImportedMethods.XWF_GetVSProp(
                VolumeSnapshotPropertyType.SpecialItemID
                , SpecialItemType.SystemVolumeInformationDirectory);
            // Get the Windows EDB file.
            Properties.windowsEDBFile = ImportedMethods.XWF_GetVSProp(
                VolumeSnapshotPropertyType.SpecialItemID
                , SpecialItemType.WindowsEDBFile);
            // Get the first hash.
            Properties.hashType1 = (HashType)ImportedMethods.XWF_GetVSProp(
                VolumeSnapshotPropertyType.HashType1
                , SpecialItemType.Ununsed);
            // Get the second hash.
            Properties.hashType2 = (HashType)ImportedMethods.XWF_GetVSProp(
                VolumeSnapshotPropertyType.HashType2
                , SpecialItemType.Ununsed);

            return Properties;
        }

        /// <summary>
        /// Retrieves the number of items in the current volume snapshot (both files and
        /// directories). Item IDs are consecutive 0-based, meaning the ID of the first
        /// item in the volume snapshot is 0 and the last item is GetItemCount-1. You
        /// address each and every item in that range, be it a file or directory, by 
        /// specifying its ID. A helper method for XWF_GetItemCount().
        /// </summary>
        /// <returns>Returns the number of file and directories in the curent volume
        /// snapshot.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static uint GetItemCount()
        {
            return ImportedMethods.XWF_GetItemCount(IntPtr.Zero);
        }

        /// <summary>
        /// Retrieves the accumulated number of files in the directory with the 
        /// specified ID and all its subdirectories. Also works for files that have
        /// child objects. Not currently supported for the root directory though you may
        /// specify -1 as the ID to get the total file count of the entire volume.
        /// Available from v17.7. A helper method for XWF_GetFileCount().
        /// </summary>
        /// <param name="directoryId">The directory ID.</param>
        /// <returns>Returns the accumulated number of all files under a provided
        /// directory or file and all it's subdirectories</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static uint GetFileCount(uint directoryId)
        {
            return ImportedMethods.XWF_GetFileCount(directoryId);
        }

        /// <summary>
        /// Creates a new item (file or directory) in the volume snapshot. May be called
        /// when refining the volume snapshot. Should be followed by calls 
        /// to XWF_SetItemParent, XWF_SetItemSize, XWF_SetItemInformation, and/or 
        /// XWF_SetItemOfs. If via XWF_SetItemParent, you make the new file a child
        /// object of a file (not directory), you are responsible for setting the 
        /// parent's XWF_ITEM_INFO_FLAG_HASCHILDREN flag. For example, if you are
        /// creating a file carved from the sectors of the evidence object, you can
        /// specify the file size using XWF_SetItemSize and the start offset via the
        /// nDefOfs parameter (must be negative) using XWF_SetItemOfs. A helper method
        /// for XWF_CreateItem().
        /// </summary>
        /// <param name="itemName">The name of the item.</param>
        /// <param name="options">Creation flags (XWFCreateItemFlags).</param>
        /// <returns>Returns the ID of the newly created item, or -1 if an error
        /// occurred (e.g. out of memory).</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Not sure we need the marshalled parameter.</remarks>
        public static int CreateItem([MarshalAs(UnmanagedType.LPWStr)] string itemName
            , CreateItemOptions options)
        {
            return ImportedMethods.XWF_CreateItem(itemName, options);
        }

        /// <summary>
        /// Similar to XWF_CreateItem, but also allows attachment of an external file to
        /// the volume snapshot or to define a file that is an excerpt of another file
        /// (its parent). Returns the ID of the newly created item, or -1 if an error 
        /// occurred (e.g. out of memory). Should be followed by a call to 
        /// XWF_SetItemSize (not necessary if attaching an external file) or 
        /// XWF_SetItemInformation (not necessary when carving a file in a file).
        /// Available from v16.7. A helper method for XWF_CreateFile().
        /// </summary>
        /// <param name="fileName">The name that this file will have in the volume 
        /// snapshot, which may be different from its source file name if you are
        /// attaching an external file.</param>
        /// <param name="options">Creation flags (XWFCreateFileFlags).</param>
        /// <param name="parentItemId">The file's parent ID, if needed.</param>
        /// <param name="pSourceInfo">More information about the source of the file's
        /// data. The exact meaning depends on the flags.</param>
        /// <returns>Returns the Id of the newly created item or -1 if an error occurred.
        /// </returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Not sure we need the marshalled parameter.</remarks>
        public static int CreateFile([MarshalAs(UnmanagedType.LPWStr)] string fileName
            , CreateFileOptions options, uint parentItemId, IntPtr sourceInformation)
        {
            return ImportedMethods.XWF_CreateFile(fileName, options, parentItemId
                , sourceInformation);
        }

        /// <summary>
        /// Retrieves a pointer to the null-terminated name of the specified item (file
        /// or directory) in UTF-16. You may call XWF_GetItemName and XWF_GetItemParent
        /// repeatedly until you reach the root directory and concatenate the results to
        /// get the full path of an item. A helper method for XWF_GetItemName().
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        /// <returns>Returns name of the item.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Implement path builder helper method.
        /// - Todo: Currently catching all exceptions; need to test further.</remarks>
        public static string GetItemName(int itemId)
        {
            string Result;

            try
            {
                IntPtr Buffer = ImportedMethods.XWF_GetItemName(itemId);
                Result = Marshal.PtrToStringUni(Buffer);
                Marshal.FreeHGlobal(Buffer);
            }
            catch (Exception e)
            {
                OutputMessage("Exception: " + e);
                return null;
            }

            return Result;
        }

        /// <summary>
        /// Retrieves the size of the item (file or directory) in bytes, or -1 when the
        /// size is unknown. A helper method for XWF_GetItemSize().
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        /// <returns>Returns the size of the item, or -1 if unknown.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static long GetItemSize(long itemId)
        {
            return ImportedMethods.XWF_GetItemSize(itemId);
        }

        /// <summary>
        /// Sets the size of the item in bytes, using -1 when the size is unknown. A
        /// helper method for XWF_SetItemSize().
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        /// <param name="size">The size of the item, or -1 if unknown.</param>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static void SetItemSize(int itemId, int size)
        {
            ImportedMethods.XWF_SetItemSize(itemId, size);
        }

        /// <summary>
        /// Retrieves the offset of the file system data structure (e.g. NTFS FILE 
        /// record) where the item is defined. If negative, the absolute value is the 
        /// offset where a carved file starts on the volume. 0 if an error occurred. 
        /// 0xFFFFFFFF if not available/not applicable. Also retrieves the number of the 
        /// sector from the point of the volume in which the data of the item starts. A
        /// helper method for XWF_GetItemOfs().
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        /// <returns>Returns XWFItemOffsets struct with the relative offsets.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static ItemOffsets GetItemOffsets(int itemId)
        {
            long ItemOffset, StartSector;
            ItemOffsets ItemOffsets = new ItemOffsets();

            ImportedMethods.XWF_GetItemOfs(itemId, out ItemOffset, out StartSector);

            if (ItemOffset >= 0)
            {
                ItemOffsets.FileSystemDataStructureOffset = ItemOffset;
                ItemOffsets.CarvedFileVolumeOffset = -1;
            }
            else
            {
                ItemOffsets.FileSystemDataStructureOffset = -1;
                ItemOffsets.CarvedFileVolumeOffset = Math.Abs(ItemOffset);
            }

            ItemOffsets.DataStartSector = StartSector;

            return ItemOffsets;
        }

        /// <summary>
        /// Sets the offset and data sector start of a given item. A helper method for
        /// XWF_SetItemOfs().
        /// </summary>
        /// <param name="ItemId">The item ID.</param>
        /// <param name="itemOffsets">A ItemOffsets struct with the offsets to use.
        /// </param>
        /// <returns>Returns true if successful, otherwise false.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static bool SetItemOffsets(int itemId, ItemOffsets itemOffsets)
        {
            long itemOffset;

            if (itemOffsets.FileSystemDataStructureOffset != -1)
            {
                itemOffset = itemOffsets.FileSystemDataStructureOffset;
            }
            else if (itemOffsets.CarvedFileVolumeOffset != -1)
            {
                itemOffset = itemOffsets.CarvedFileVolumeOffset * -1;
            }
            else
            {
                return false;
            }

            ImportedMethods.XWF_SetItemOfs(itemId, itemOffset
                , itemOffsets.DataStartSector);

            return true;
        }

        /// <summary>
        /// Returns information about an item (file or directory) as stored in the 
        /// volume snapshot, such as the original ID or attributes that the item had in 
        /// its defining file system. A helper method for XWF_GetItemInformation().
        /// </summary>
        /// <param name="itemId">The item Id.</param>
        /// <returns>Returns ItemInformation struct with the given item's information.
        /// </returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static ItemInformation GetItemInformation(int itemId)
        {
            ItemInformation Information = new ItemInformation();
            bool Status;

            // Get the original Id.
            Information.OriginalItemID = ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_ORIG_ID, out Status);

            // Get the attributes.
            Information.Attributes = ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_ATTR, out Status);

            // Get the flags.
            Information.Flags = (ItemInformationOptions)
                ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_FLAGS, out Status);

            // Get the deletion information.
            Information.Deletion = (ItemDeletionStatus)
                ImportedMethods.XWF_GetItemInformation(itemId
                    , ItemInformationType.XWF_ITEM_INFO_DELETION, out Status);

            // Get the classification.
            Information.Classification = (ItemClassifiction)
                ImportedMethods.XWF_GetItemInformation(itemId
                    , ItemInformationType.XWF_ITEM_INFO_CLASSIFICATION, out Status);

            // Get the link count.
            Information.LinkCount = ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_LINKCOUNT, out Status);

            // Get the color analysis.
            Information.ColorAnalysis = ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_COLORANALYSIS, out Status);

            // Get the file count.
            Information.FileCount = ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_FILECOUNT, out Status);

            // Get the embedded offset.
            Information.EmbeddedOffset = ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_EMBEDDEDOFFSET, out Status);

            // Get the creation time.
            Information.CreationTime = DateTime.FromFileTime(
                ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_CREATIONTIME, out Status));

            // Get the modification time.
            Information.ModificationTime = DateTime.FromFileTime(
                ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_MODIFICATIONTIME, out Status));

            // Get the last access time.
            Information.LastAccessTime = DateTime.FromFileTime(
                ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_LASTACCESSTIME, out Status));

            // Get the entry modification time.
            Information.EntryModificationTime = DateTime.FromFileTime(
                ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_ENTRYMODIFICATIONTIME
                , out Status));

            // Get the deletion time.
            Information.DeletionTime = DateTime.FromFileTime(
                ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_DELETIONTIME, out Status));

            // Get the internal creation time.
            Information.InternalCreationTime = DateTime.FromFileTime(
                ImportedMethods.XWF_GetItemInformation(itemId
                , ItemInformationType.XWF_ITEM_INFO_INTERNALCREATIONTIME
                , out Status));

            return Information;
        }

        /// <summary>
        /// Sets information about an item (file or directory) in the volume snapshot. A
        /// helper method for XWF_SetItemInformation().
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="informationType"></param>
        /// <param name="informationValue"></param>
        /// <remarks>Todo: Everything!</remarks>
        public static void SetItemInformation(int itemId
            , ItemInformationType informationType, long informationValue)
        {
        }

        /// <summary>
        /// Retrieves a textual description of the type of the specified file and 
        /// returns information about the status of the type detection of the file: 
        /// 0 = not verified, 1 = too small, 2 = totally unknown, 3 = confirmed, 
        /// 4 = not confirmed, 5 = newly identified, 6 (v18.8 and later only) = mismatch
        /// detected. ­1 means error. A helper method for XWF_GetItemType().
        /// </summary>
        /// <param name="itemId">The item Id.</param>
        /// <returns>Returns a ItemType structure with the file type and description.
        /// </returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static ItemType GetItemType(int itemId)
        {
            ItemType Results = new ItemType();

            // Allocate a buffer to receive the type description.
            IntPtr bufferPtr = Marshal.AllocHGlobal(_itemTypeDescriptionBufferLength);

            // Get the results from the API function, including the type description.
            Results.Type = ImportedMethods.XWF_GetItemType(itemId, bufferPtr
                , _itemTypeDescriptionBufferLength);
            Results.Description = Marshal.PtrToStringUni(bufferPtr);
            Marshal.FreeHGlobal(bufferPtr);

            return Results;
        }

        /// <summary>
        /// Sets a description of the type of the specified file (or specify NULL if not 
        /// required) and information about the status of the type detection of the file.
        /// A helper method for XWF_SetItemType().
        /// </summary>
        /// <param name="itemId">The item Id.</param>
        /// <param name="typeDescription">A type description.</param>
        /// <param name="itemType">The item type category.</param>
        /// <returns></returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Catching all exceptions; need to investigate possibilities.
        /// - Todo: Convert to static method.</remarks>
        public static bool SetItemType(int itemId, string typeDescription
            , ItemTypeCategory itemType)
        {
            try
            {
                ImportedMethods.XWF_SetItemType(itemId, typeDescription, itemType);
            }
            catch (Exception e)
            {
                OutputMessage("Exception: " + e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the ID of the parent of the specified item, or -1 if the item is the 
        /// root directory or if for some strange reason no parent object is assigned. A
        /// helper method for XWF_GetItemParent().
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        /// <returns>Returns the parent ID of the given item, or -1 if there is none.
        /// </returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static int GetItemParent(int itemId)
        {
            return ImportedMethods.XWF_GetItemParent(itemId);
        }

        /// <summary>
        /// Sets the parent of a given child item. A helper method for 
        /// XWF_SetItemParent().
        /// </summary>
        /// <param name="childItemID">The child ID.</param>
        /// <param name="parentItemID">The parent ID.</param>
        /// <returns>Return true is successful, otherwise false.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Catching all exceptions; need to invetigate possibilities.
        /// - Todo: What happens when invalid child or parents is given?</remarks>
        public static bool SetItemParent(int childItemID, int parentItemID)
        {
            try
            {
                ImportedMethods.XWF_SetItemParent(childItemID, parentItemID);
            }
            catch (Exception e)
            {
                OutputMessage("Exception: " + e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves the names of the report tables that the specified item is 
        /// associated with. The names are delimited with a comma and space. If the 
        /// buffer was filled completely, that likely means the specified buffer length 
        /// was insufficient. In v17.6 SR-7 and later, returns the total number of
        /// associations of that file, and lpBuffer may be NULL. A helper method for
        /// XWF_GetReportTableAssocs().
        /// </summary>
        /// <param name="itemId">The ID of the provided item.</param>
        /// associated with.</param>
        /// <returns>Returns the number of associations of the given item.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static string[] GetReportTableAssociations(int itemId)
        {
            const int BufferLengthStep = 128;
            string Associations;

            //
            for (int bufferLength = BufferLengthStep; ; bufferLength += BufferLengthStep)
            {
                // Allocate a buffer to receive the associations.
                IntPtr Buffer = Marshal.AllocHGlobal(bufferLength);

                // Get the results from the API function, including the associations up
                // the specified buffer length.
                int AssociationsCount = ImportedMethods.XWF_GetReportTableAssocs(itemId
                    , Buffer, bufferLength);

                // If no associations, empty the associations string and return.
                if (AssociationsCount <= 0)
                {
                    return new string[0];
                }

                // Get a string representation of the associations buffer.
                string Str = Marshal.PtrToStringUni(Buffer, bufferLength);
                Marshal.FreeHGlobal(Buffer);

                // Check for a NULL character and continue in the loop if not found
                int NullCharacterIndex = Str.IndexOf((char)0);
                if (NullCharacterIndex < 0 || NullCharacterIndex >= bufferLength - 1)
                    continue;

                Associations = Str.Substring(0, NullCharacterIndex);

                return Associations.Split(new string[] { ", " }
                    , StringSplitOptions.None);
            }
        }

        /// <summary>
        /// Associates the specified file with the specified report table. If the report 
        /// table does not exist yet in the currently active case, it will be created. 
        /// A helper method for XWF_AddToReportTable().
        /// </summary>
        /// <param name="itemId">The Id of the item to association with the report table.
        /// </param>
        /// <param name="reportTableName">The report table name.</param>
        /// <param name="options">Options to use for the association.</param>
        /// <returns>Returns the result of the assocation.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Needs testing.</remarks>
        public static AddToReportTableResult AddToReportTable(int itemId
            , string reportTableName, AddToReportTableOptions options)
        {
            return ImportedMethods.XWF_AddToReportTable(itemId, reportTableName,
                options);
        }

        /// <summary>
        /// Gets the comment (if any) of the given item. A helper method for 
        /// XWF_GetComment().
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        /// <returns>Returns the comment.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Currently catching all exceptions; need to figure out what the
        /// possible exceptions are.</remarks>
        public static string GetComment(int itemId)
        {
            string Comment;

            try
            {
                IntPtr Buffer = ImportedMethods.XWF_GetComment(itemId);
                Comment = Marshal.PtrToStringUni(Buffer);
                Marshal.FreeHGlobal(Buffer);
            }
            catch (Exception e)
            {
                OutputMessage("Exception: " + e);
                return null;
            }

            return Comment;
        }

        /// <summary>
        /// Sets the comment of the given item. A helper method for XWF_AddComment().
        /// </summary>
        /// <param name="itemId">The item Id.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="mode">Indicates how the comment should be added.</param>
        /// <returns>Returns true if successfull, otherwise false.</returns>
        /// <remarks>Version 1.0 coding complete.</remarks>
        public static bool AddComment(int itemId, string comment, AddCommentMode mode)
        {
            return ImportedMethods.XWF_AddComment(itemId, comment, mode);
        }

        /// <summary>
        /// Get the previously extracted metadata of a given item.  Good to use this one 
        /// if metadata has already been extracted from items. Available in v17.7 and 
        /// later. A helper method for XWF_GetExtractedMetadata().
        /// </summary>
        /// <param name="itemId">The item Id.</param>
        /// <returns>Returns the previously extracted metadata.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Needs testing.
        /// - Todo: Catching all exceptions, need to determine exception possiblities.
        /// </remarks>
        public static string GetExtractedMetadata(int itemId)
        {
            string Metadata;

            try
            {
                IntPtr Buffer = ImportedMethods.XWF_GetExtractedMetadata(itemId);
                Metadata = Marshal.PtrToStringUni(Buffer);
                Marshal.FreeHGlobal(Buffer);
            }
            catch (Exception e)
            {
                OutputMessage("Exception: " + e);
                return null;
            }

            return Metadata;
        }

        /// <summary>
        /// Adds the specified text to the extracted metadata of the specified item. 
        /// Available in v17.7 and later. A helper method for XWF_AddExtractedMetadata().
        /// </summary>
        /// <param name="itemId">The item Id.</param>
        /// <param name="text">The text to add to the extracted metadata.</param>
        /// <param name="mode">Indicates how the metadata should be added.</param>
        /// <returns>Returns true if successfull, otherwise false.</returns>
        /// <remarks>Verion 1.0 coding complete.
        /// - Todo: Test the method.</remarks>
        public static bool AddExtractedMetadata(int itemId, string text
            , AddCommentMode mode)
        {
            return ImportedMethods.XWF_AddExtractedMetadata(itemId, text, mode);
        }

        /// <summary>
        /// Retrieves the hash value of a a file if one has been computed, which can be 
        /// checked using GetItemInformation(). Available in v16.8 and later. A helper 
        /// method for XWF_GetHashValue().
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        /// <remarks>
        /// - Todo: Needs testing and a lot of work.
        /// - Todo: Check version
        /// - Todo: Define variable for the buffer lenght.</remarks>
        public static string GetHashValue(int itemId)
        {
            string Hash;
            IntPtr Buffer = Marshal.AllocHGlobal(_volumeNameBufferLength);
            ImportedMethods.XWF_GetHashValue(itemId, Buffer);
            Hash = Marshal.PtrToStringUni(Buffer);
            Marshal.FreeHGlobal(Buffer);
            return Hash;
        }

        /// <summary>
        /// Extracts internal metadata of a file to memory and returns a pointer to it if 
        /// successful, or NULL otherwise. The pointer is guaranteed to be valid only at 
        /// the time when you retrieve it. If you wish to do something with the text that 
        /// it points to after your X-Tension returns control to X-Ways Forensics, you 
        /// need to copy it to your own buffer. Unlike GetExtractedMetadata, the file 
        /// must have been opened with XWF_OpenItem because this function reads from the 
        /// file contents, not from data stored in the volume snapshot. The metadata is 
        /// taken from the very file that contains it, for example in the case of zip-
        /// style Office documents from the XML files. Available in v17.7 and later. A 
        /// helper method for XWF_GetMetadata().
        /// </summary>
        /// <param name="itemId">The item Id.</param>
        /// <returns>Returns the metadata if successful, or NULL otherwise.</returns>
        /// <remarks>Version 1.0 coding complete.
        /// - Todo: Needs some serious work and testing.
        /// - Question: What buffer length should be used?</remarks>
        public static string GetMetadata(int itemId)
        {
            string Metadata;
            IntPtr Buffer = Marshal.AllocHGlobal(_volumeNameBufferLength);
            ImportedMethods.XWF_GetMetadata(itemId, Buffer);
            Metadata = Marshal.PtrToStringUni(Buffer);
            Marshal.FreeHGlobal(Buffer);
            return Metadata;
        }

        /// <summary>
        /// Provides a standardized true-color RGB raster image representation for any 
        /// picture file type that is supported internally in X-Ways Forensics (e.g. 
        /// JPEG, GIF, PNG, ...), with 24 bits per pixel. The result is a pointer to a 
        /// memory buffer, or NULL if not successful (e.g. if not a supported file type 
        /// variant or the file is too corrupt). The caller is responsible for releasing 
        /// the allocated memory buffer when no longer needed, by calling the Windows API 
        /// function VirtualFree, with parameters dwSize = 0 and dwFreeType = 
        /// MEM_RELEASE. Available in v18.0 and later. A helper method for 
        /// XWF_GetRasterImage().
        /// </summary>
        /// <param name="ImageInformation">A structure of image information.</param>
        /// <returns>Returns a pointer to the raster image.</returns>
        /// <remarks>Todo: Everything.</remarks>
        public static IntPtr GetRasterImage(RasterImageInformation imageInformation)
        {
            return IntPtr.Zero;
        }

        /// <summary>
        /// Runs a simultaneous search for multiple search terms in the specified volume. 
        /// The volume must be associated with an evidence object. Note that if this 
        /// function is called as part of volume snapshot refinement, it can be called 
        /// automatically for all selected evidence objects if the user applies the 
        /// X-Tension to all selected evidence objects. Must only be called from 
        /// XT_Prepare or XT_Finalize. Available in v16.5 and later. A wrapper method for 
        /// XWF_Search().
        /// </summary>
        /// <param name="information">Information about the search.</param>
        /// <param name="codePages">The code pages to use.</param>
        /// <returns></returns>
        public static int Search(ref SearchInformation information, CodePages codePages)
        {
            return 0;
        }

        /// <summary>
        /// Creates a new search term and returns its ID or (if flag 0x01 is specified) 
        /// alternatively returns the ID of an existing search term with the same name, 
        /// if any. Returns -1 in case of an error. The maximum number of search terms in 
        /// a case is currently 8,191 (in v18.5). Use this function if you wish to 
        /// automatically categorize search hits (assign them to different search terms) 
        /// while responding to calls of ProcessSearchHit() or using SetSearchHit(). 
        /// Available in v18.5 and later. A helper method for XWF_AddSearchTerm().
        /// </summary>
        /// <param name="SearchTermName"></param>
        /// <returns></returns>
        public static int AddSearchTerm(string SearchTermName)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SInfo"></param>
        /// <returns></returns>
        public static int XWFSearchWithoutCodePages(ref SearchInformation SInfo)
        {
            return ImportedMethods.XWF_SearchWithPtrToPages(ref SInfo, IntPtr.Zero);
        }

        /// <summary>
        /// A helper method for XWF_OutputMessage().
        /// </summary>
        /// <param name="lpMessage"></param>
        /// <param name="level"></param>
        /// <param name="nFlags"></param>
        public static void OutputMessage(
            [MarshalAs(UnmanagedType.LPWStr)] string lpMessage
            , OutputMessageLevel level = OutputMessageLevel.Level1
            , OutputMessageOptions nFlags = OutputMessageOptions.None)
        {
            string tab = new string(' ', (int)level * 4);
            ImportedMethods.OutputMessage(tab + lpMessage, nFlags);
        }

        public static void OutputEmptyLine()
        {
            OutputMessage("");
        }

        public static void OutputHeader(
            [MarshalAs(UnmanagedType.LPWStr)] string lpMessage
            , OutputMessageLevel level = OutputMessageLevel.Level1)
        {
            OutputMessage(lpMessage, level);
            OutputMessage("");
        }

        public static string Hexlify(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        public static ArrayList GetCaseEvidence()
        {
            ArrayList evidence = new ArrayList();

            IntPtr hCurrent = GetFirstEvidenceObject();

            while (hCurrent != IntPtr.Zero)
            {
                evidence.Add(hCurrent);
                hCurrent = GetNextEvidenceObject(hCurrent);
            }

            return evidence;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hItem"></param>
        /// <returns></returns>
        public static byte[] ReadItem(IntPtr hItem)
        {
            //If successful - returns contents of the item as a byte array,
            //if failed - returns null.

            //if (ImportedMethods.XWFGetSize != null && ImportedMethods.XWFRead != null)
            //if (ImportedMethods.XWF_Read != null)
            try
            {
                // Get the size of the provided item.
                long size = GetSize(hItem, ItemSizeType.PhysicalSize);

                // Initialize and create a pointer to the buffer.
                int bufferSize = (int)size;
                IntPtr bufferPtr = Marshal.AllocHGlobal(bufferSize);

                // Call XWF_Read to read the item into the buffer.
                return Read(hItem, 0, bufferSize);
            }
            catch { }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchTerms"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static SearchInformation CreateSearchInfo(string searchTerms
            , SearchInformationOptions flags)
        {
            SearchInformation info = new SearchInformation
            {
                hVolume = IntPtr.Zero //the docs say that hVolume should be 0
                ,
                lpSearchTerms = searchTerms
                ,
                nFlags = flags
                ,
                nSearchWindow = 0
            };

            info.iSize = Marshal.SizeOf(info);
            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static string GetFullPath(int itemId)
        {
            /*
            from the docs:
            
            XWF_GetItemParent returns the ID of the parent of the specified item,
            or -1 if the item is the root directory.                         
            */

            StringBuilder sb = new StringBuilder();
            while (true)
            {
                int parentItemId = HelperMethods.GetItemParent(itemId);

                /*
                XWFGetItemName returns text "(Root directory)" for the root directory.
                I don't see any sense in putting such kind of a string into the path,
                so, if (parentItemId < 0) then this is a root directory
                and we don't need it's name to be added.
                */
                if (parentItemId < 0) return sb.ToString();

                sb.Insert(0, Path.DirectorySeparatorChar
                    + GetItemName(itemId));

                itemId = parentItemId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="externalFilename"></param>
        /// <param name="parentItemId"></param>
        /// <param name="keepExternalFile"></param>
        /// <returns></returns>
        public static int CreateFileFromExternalFile(string name
            , string externalFilename
            , uint parentItemId
            , bool keepExternalFile = false)
        {
            IntPtr extFilenamePtr = Marshal.StringToHGlobalUni(externalFilename);

            int itemId = CreateFile(name
                , CreateFileOptions.AttachExternalFile
                    | (keepExternalFile ? CreateFileOptions.KeepExternalFile : 0)
                , parentItemId
                , extFilenamePtr);

            Marshal.FreeHGlobal(extFilenamePtr);
            return itemId;
        }
    }
}
