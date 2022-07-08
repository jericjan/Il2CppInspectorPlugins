using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NoisyCowStudios.Bin2Object;
using Il2CppInspector;
using Il2CppInspector.PluginAPI;
using Il2CppInspector.PluginAPI.V100;
using System.Diagnostics;
using System.Text;
using Il2CppInspector.Model;
using Il2CppInspector.Reflection;

namespace Loader
{
//copied from MappedTypes.cs
    public class Il2CppGlobalMetadataHeader
    {
        [SkipWhenReading]
        public uint signature = Il2CppConstants.MetadataSignature;

        [SkipWhenReading]
        public int version = 24;

        [ArrayLength(FixedSize = 0x28)]
        public byte[] unk;

        public int genericContainersOffset; // Il2CppGenericContainer
        public int genericContainersCount;
        public int nestedTypesOffset; // TypeDefinitionIndex
        public int nestedTypesCount;
        public int interfacesOffset; // TypeIndex
        public int interfacesCount;
        public int vtableMethodsOffset; // EncodedMethodIndex
        public int vtableMethodsCount;
        public int interfaceOffsetsOffset; // Il2CppInterfaceOffsetPair
        public int interfaceOffsetsCount;
        public int typeDefinitionsOffset; // Il2CppTypeDefinition
        public int typeDefinitionsCount;

        public int rgctxEntriesOffset; // Il2CppRGCTXDefinition
        public int rgctxEntriesCount;

        public int unk1;
        public int unk2;
        public int unk3;
        public int unk4;

        public int imagesOffset; // Il2CppImageDefinition
        public int imagesCount;
        public int assembliesOffset; // Il2CppAssemblyDefinition
        public int assembliesCount;

        public int fieldsOffset; // Il2CppFieldDefinition
        public int fieldsCount;
        public int genericParametersOffset; // Il2CppGenericParameter
        public int genericParametersCount;

        public int fieldAndParameterDefaultValueDataOffset; // uint8_t
        public int fieldAndParameterDefaultValueDataCount;

        public int fieldMarshaledSizesOffset; // Il2CppFieldMarshaledSize
        public int fieldMarshaledSizesCount;
        public int referencedAssembliesOffset; // int32_t
        public int referencedAssembliesCount;

        public int attributesInfoOffset; // Il2CppCustomAttributeTypeRange
        public int attributesInfoCount;
        public int attributeTypesOffset; // TypeIndex
        public int attributeTypesCount;

        public int unresolvedVirtualCallParameterTypesOffset; // TypeIndex
        public int unresolvedVirtualCallParameterTypesCount;
        public int unresolvedVirtualCallParameterRangesOffset; // Il2CppRange
        public int unresolvedVirtualCallParameterRangesCount;

        public int windowsRuntimeTypeNamesOffset; // Il2CppWindowsRuntimeTypeNamePair
        public int windowsRuntimeTypeNamesSize;
        public int exportedTypeDefinitionsOffset; // TypeDefinitionIndex
        public int exportedTypeDefinitionsCount;

        public int unk5;
        public int unk6;

        public int parametersOffset; // Il2CppParameterDefinition
        public int parametersCount;

        public int genericParameterConstraintsOffset; // TypeIndex
        public int genericParameterConstraintsCount;

        public int unk7;
        public int unk8;

        public int metadataUsagePairsOffset; // Il2CppMetadataUsagePair
        public int metadataUsagePairsCount;

        public int unk9;
        public int unk10;
        public int unk11;
        public int unk12;

        public int fieldRefsOffset; // Il2CppFieldRef
        public int fieldRefsCount;

        public int eventsOffset; // Il2CppEventDefinition
        public int eventsCount;
        public int propertiesOffset; // Il2CppPropertyDefinition
        public int propertiesCount;
        public int methodsOffset; // Il2CppMethodDefinition
        public int methodsCount;

        public int parameterDefaultValuesOffset; // Il2CppParameterDefaultValue
        public int parameterDefaultValuesCount;

        public int fieldDefaultValuesOffset; // Il2CppFieldDefaultValue
        public int fieldDefaultValuesCount;

        public int unk13;
        public int unk14;
        public int unk15;
        public int unk16;

        public int metadataUsageListsOffset; // Il2CppMetadataUsageList
        public int metadataUsageListsCount;
    }

    public class Il2CppTypeDefinition
    {
        public int nameIndex;
        public int namespaceIndex;
        public int customAttributeIndex;
        public int byvalTypeIndex;
        public int byrefTypeIndex;

        public int declaringTypeIndex;
        public int parentIndex;
        public int elementTypeIndex;

        public int rgctxStartIndex;
        public int rgctxCount;

        public int genericContainerIndex;

        public uint flags;

        public int fieldStart;
        public int propertyStart;
        public int methodStart;
        public int eventStart;
        public int nestedTypesStart;
        public int interfacesStart;
        public int interfaceOffsetsStart;
        public int vtableStart;

        public ushort event_count;
        public ushort method_count;
        public ushort property_count;
        public ushort field_count;
        public ushort vtable_count;
        public ushort interfaces_count;
        public ushort interface_offsets_count;
        public ushort nested_type_count;

        public uint bitfield;
        public uint token;
    }

    public class Il2CppMethodDefinition
    {
        public int returnType;
        public int declaringType;
        public int unk1;
        public int nameIndex;
        public int parameterStart;
        public int genericContainerIndex;
        public int customAttributeIndex;
        public int reversePInvokeWrapperIndex;
        public int unk2;
        public int methodIndex;
        public int invokerIndex;
        public int rgctxCount;
        public int rgctxStartIndex;
        public ushort parameterCount;
        public ushort flags;
        public ushort slot;
        public ushort iflags;
        public uint token;
    }

    public class Il2CppFieldDefinition
    {
        public int customAttributeIndex;
        public int typeIndex;
        public int nameIndex;
        public uint token;
    }

    public class Il2CppPropertyDefinition
    {
        public int customAttributeIndex;
        public int nameIndex;
        public int unk1;
        public uint token;
        public uint attrs;
        public int unk2;
        public int set;
        public int get;
    }



// Define your plugin class, implementing IPlugin plus interfaces for any hooks you wish to use
public class Plugin : IPlugin, ILoadPipeline
    {
        // Win32 API imports
        // Here we import functions from non-.NET DLLs that we need for our task

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string name);

        // Delegates to functions in UnityPlayer.dll
        // These delegates enable us to call unmanaged functions using .NET syntax

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr DecryptMetadata(byte[] bytes, int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetStringFromIndex(byte[] bytes, uint index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetStringLiteralFromIndex(byte[] bytes, uint index, ref int length);

        // Set the details of the plugin here
        public string Id => "honkai";
        public string Name => "Honkai Loader";
        public string Author => "Kur0";
        public string Version => "1.0.0";
        public string Description => "Enables loading of Honkai Impact 3 games";

        // Options

        // We'll need the file path to the corresponding UnityPlayer.dll (actually Honkai Impact 3.8 and 4.3 seem to be interchangeable)
        private PluginOptionFilePath unityPath = new PluginOptionFilePath
        {
            Name = "unity-player-path",
            Description = "Path to selected UnityPlayer.dll version\n\n"
                        + "NOTE: UnityPlayer.dll from the PC release of the game is required even if you are inspecting a mobile version\n\n"
                        + "NOTE: The global-metadata.dat for some game versions can be decrypted by a different UnityPlayer.dll version.\n\n"
                        + "NOTE: Some UnityPlayer.dll versions are interchangeable, but not all. If your version isn't listed, select the closest available version.",
            Required = true,
            MustExist = true,
            AllowedExtensions = new Dictionary<string, string> { ["dll"] = "DLL files" }
        };

        private PluginOptionChoice<string> game = new PluginOptionChoice<string>
        {
            Name = "game",
            Description = "Honkai version to use for decryption",
            Required = true,
            Value = "honkai-impact-5.8",
            Choices = new Dictionary<string, string>
            {
                ["honkai-impact-5.8"] = "Honkai Impact 5.8"
            }
        };

        // Make the options available to Il2CppInspector
        public List<IPluginOption> Options => new List<IPluginOption> { game, unityPath };

        // Unity function offsets
        private class UnityOffsets
        {
            public int DecryptMetadata;
            public int GetStringFromIndex;
            public int GetStringLiteralFromIndex;
        }

        // These are the offsets to various functions in each product's UnityPlayer.dll
        // You have to find these by reverse-engineering the code yourself
        private Dictionary<string, UnityOffsets> Offsets = new Dictionary<string, UnityOffsets>
        {
            ["honkai-impact-5.8"] = new UnityOffsets { DecryptMetadata = 0xC9C10, GetStringFromIndex = 0xC7FBF0, GetStringLiteralFromIndex = 0xC7FC00 },

        };

        // Handle to the loaded DLL
        private IntPtr hModule;

        // The base address in memory of the loaded DLL
        private IntPtr ModuleBase;

        // Decrypted metadata blob - this will hold the decrypted (but not deobfuscated) internal copy of global-metadata.dat
        // so that we can pass it back to the DLL as needed
        private byte[] metadataBlob;

        // We keep a flag so that we only process applications from miHoYo and not others!
        private bool IsOurs;

        // Here we implement ILoadPipeline

        // This executes when the client begins to load a new IL2CPP application
        // Place initialization code here
        public void LoadPipelineStarting(PluginLoadPipelineStartingEventInfo info)
        {

            // Try to load UnityPlayer.dll
            hModule = LoadLibrary(unityPath.Value);

            if (hModule == IntPtr.Zero)
                throw new FileLoadException("Could not load UnityPlayer DLL", unityPath.Value);

            // Get the base address of the loaded DLL in memory
            ModuleBase = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().First(m => m.ModuleName == Path.GetFileName(unityPath.Value)).BaseAddress;
        }

        // This executes when the client finishes loading an IL2CPP application
        // Place teardown code here
        public void LoadPipelineEnding(List<Il2CppInspector.Il2CppInspector> packages, PluginLoadPipelineEndingEventInfo info)
        {

            // Release memory lock on UnityPlayer.dll
            FreeLibrary(hModule);
        }

        // This executes as soon as the raw global-metadata.dat has been read from storage,
        // before any attempt is made to analyze its contents
        // We use it to call UnityPlayer.dll to decrypt the file in memory
        public void PreProcessMetadata(BinaryObjectStream stream, PluginPreProcessMetadataEventInfo info)
        {

            // Assume that your plugin is enabled regardless of what is loading
            // Therefore, we don't want to process global-metadata.dat files that are not for us!
            // miHoYo metadata has an invalid signature at the start of the file so we use that as the criteria
            IsOurs = stream.ReadUInt32(0) != Il2CppConstants.MetadataSignature;
            if (!IsOurs)
                return;

            // The DWORD 0x4008 bytes from the end should be an offset to itself
            var lastBlockPointer = stream.Length - 0x4008;
            IsOurs = stream.ReadUInt32(lastBlockPointer) == lastBlockPointer;
            if (!IsOurs)
                return;

            // Tell the user what is happening in case it takes a while
            PluginServices.For(this).StatusUpdate("Decrypting metadata");

            // Create a delegate which internally is a function pointer to the DecryptMetadata function in the DLL
            var pDecryptMetadata = (DecryptMetadata)Marshal.GetDelegateForFunctionPointer(ModuleBase + Offsets[game.Value].DecryptMetadata, typeof(DecryptMetadata));

            // Call the delegate with the encrypted metadata byte array and length as arguments
            var decryptedBytesUnmanaged = pDecryptMetadata(stream.ToArray(), (int)stream.Length);

            // Copy the decrypted data back from unmanaged memory to a byte array
            metadataBlob = new byte[stream.Length];
            Marshal.Copy(decryptedBytesUnmanaged, metadataBlob, 0, (int)stream.Length);

            var key = new byte[] { 0x3f, 0x73, 0xa8, 0x5a, 0x08, 0x32, 0x0a, 0x33, 0x3c, 0xfa, 0xa3, 0x4e, 0x8b, 0x0c, 0x77, 0x45 };

            // The step is based on the file size
            var step = (int)(stream.Length >> 14) << 6;

            for (var pos = 0; pos < metadataBlob.Length; pos += step)
                for (var b = 0; b < 0x10; b++)
                    metadataBlob[pos + b] ^= key[b];

            // We replace the loaded global-metadata.dat with the newly decrypted version,
            // allowing Il2CppInspector to analyze it as normal
            stream.Write(0, metadataBlob);
            
            // Right decrypted data to current folder for testing
            //File.WriteAllBytes("global_metadedecrypteted.dat", metadataBlob);
            
            
            // Some types have reordered fields - these calls tell Il2CppInspector what the correct field order is
            // See MappedTypes.cs for details
            stream.AddObjectMapping(typeof(Il2CppInspector.Il2CppGlobalMetadataHeader), typeof(Il2CppGlobalMetadataHeader));
            stream.AddObjectMapping(typeof(Il2CppInspector.Il2CppTypeDefinition), typeof(Il2CppTypeDefinition));
            stream.AddObjectMapping(typeof(Il2CppInspector.Il2CppMethodDefinition), typeof(Il2CppMethodDefinition));
            stream.AddObjectMapping(typeof(Il2CppInspector.Il2CppFieldDefinition), typeof(Il2CppFieldDefinition));
            stream.AddObjectMapping(typeof(Il2CppInspector.Il2CppPropertyDefinition), typeof(Il2CppPropertyDefinition));

            // We tell Il2CppInspector that we have taken care of the metadata
            // IsStreamModified marks the original data stream as modified so that the user is able to save the changes
            // SkipValidation tells Il2CppInspector not to check this global-metadata.dat for validity;
            // if we don't set this, it will report that the metadata is invalid
            info.IsStreamModified = true;
            info.SkipValidation = true;
        }

        // This executes just as Il2CppInspector is about to read all of the .NET identifier strings (eg. type names).
        // We can use this to acquire the strings ourselves instead
        public void GetStrings(Metadata metadata, PluginGetStringsEventInfo data)
        {

            // Don't do anything if this isn't for us
            if (!IsOurs)
                return;

            // Tell the user what is happening in case it takes a while
            PluginServices.For(this).StatusUpdate("Decrypting strings");

            // miHoYo workloads use encrypted strings, and we need to know the correct string indexes
            // to pass to UnityPlayer.dll's GetStringFromIndex function.

            // To find them, we scan every definition in the metadata that refers to a string index,
            // combine them, put them in order and remove duplicates
            var stringIndexes =
                        metadata.Images.Select(x => x.nameIndex)
                        .Concat(metadata.Assemblies.Select(x => x.aname.nameIndex))
                        .Concat(metadata.Assemblies.Select(x => x.aname.cultureIndex))
                        .Concat(metadata.Assemblies.Select(x => x.aname.hashValueIndex))
                        .Concat(metadata.Assemblies.Select(x => x.aname.publicKeyIndex))
                        .Concat(metadata.Events.Select(x => x.nameIndex))
                        .Concat(metadata.Fields.Select(x => x.nameIndex))
                        .Concat(metadata.Methods.Select(x => x.nameIndex))
                        .Concat(metadata.Params.Select(x => x.nameIndex))
                        .Concat(metadata.Properties.Select(x => x.nameIndex))
                        .Concat(metadata.Types.Select(x => x.nameIndex))
                        .Concat(metadata.Types.Select(x => x.namespaceIndex))
                        .Concat(metadata.GenericParameters.Select(x => x.nameIndex))
                        .OrderBy(x => x)
                        .Distinct()
                        .ToList();

            // Create a delegate which internally is a function pointer to the GetStringFromIndex function in the DLL
            var pGetStringFromIndex = (GetStringFromIndex)
                Marshal.GetDelegateForFunctionPointer(ModuleBase + Offsets[game.Value].GetStringFromIndex, typeof(GetStringFromIndex));

            // For each index, call the delegate with the decrypted metadata byte array and index as arguments
            foreach (var index in stringIndexes)
                data.Strings.Add(index, Marshal.PtrToStringAnsi(pGetStringFromIndex(metadataBlob, (uint)index)));

            // This tells Il2CppInspector we have handled the strings and not to attempt to read them itself
            // The strings will be copied from data.Strings to metadata.Strings automatically
            data.IsDataModified = true;
        }

        // This executes just as Il2CppInspector is about to read all of the constant literal strings used in the application
        // We can use this to acquire the string literals ourselves instead

        // String literals are indexed from 0-n, however we don't currently know the value of n,
        // and we won't be able to calculate it until the application binary has also been processed.

        // Instead, we simply tell Il2CppInspector we have handled the string literals but actually do nothing,
        // and defer this task until later
        public void GetStringLiterals(Metadata metadata, PluginGetStringLiteralsEventInfo data)
        {

            // Don't do anything if this isn't for us
            if (!IsOurs)
                return;

            // We need to prevent Il2CppInspector from attempting to read string literals from the metadata file
            // until we can calculate how many there are
            data.FullyProcessed = true;
        }

        // A "package" is the combination of global-metadata.dat, the application binary,
        // and some analysis which links them both together into a single unit, the Il2CppInspector object.

        // This executes after all the low-level processing and analysis of the application is completed,
        // but before any higher-level abstractions are created, such as the .NET type model or C++ application model.

        // Therefore this is a good place to make any final changes to the data that the high level models and output modules will rely on
        // In this case, we are going to acquire all of the string literals that we deferred earlier
        public void PostProcessPackage(Il2CppInspector.Il2CppInspector package, PluginPostProcessPackageEventInfo data)
        {

            // Don't do anything if this isn't for us
            if (!IsOurs)
                return;

            // Tell the user what is happening in case it takes a while
            PluginServices.For(this).StatusUpdate("Decrypting string literals");

            // Calculate the number of string literals
            // This calculation depends on being able to scan MetadataUsages for all of the StringLiteral uses
            // and finding the one with the highest index. The creation of MetadataUsages requires data
            // from both global-metadata.dat and the application binary; this data is merged together
            // when the Il2CppInspector object is initialized, so this is the earliest opportunity we have to examine it
            var stringLiteralCount = package.MetadataUsages.Where(u => u.Type == MetadataUsageType.StringLiteral).Max(u => u.SourceIndex) + 1;

            // Create a delegate which internally is a function pointer to the GetStringLiteralFromIndex function in the DLL
            var pGetStringLiteralFromIndex = (GetStringLiteralFromIndex)
                Marshal.GetDelegateForFunctionPointer(ModuleBase + Offsets[game.Value].GetStringLiteralFromIndex, typeof(GetStringLiteralFromIndex));

            var stringLiterals = new List<string>();
            var length = 0;

            // For each index, call the delegate with the decrypted metadata byte array, index and a pointer as arguments
            // In this case, the function returns an array of UTF8-encoded characters,
            // and populates 'length' with the number of bytes returned
            for (uint index = 0; index < stringLiteralCount; index++)
            {
                var decryptedBytesUnmanaged = pGetStringLiteralFromIndex(metadataBlob, index, ref length);
                var str = new byte[length];
                Marshal.Copy(decryptedBytesUnmanaged, str, 0, length);

                stringLiterals.Add(Encoding.UTF8.GetString(str));
            }

            // If we had used IGetStringLiterals above, we would have set data.StringLiterals,
            // but here we modify the package (the Il2CppInspector object) directly instead
            package.Metadata.StringLiterals = stringLiterals.ToArray();

            // We don't set FullyProcessed so that other plugins can perform further post-processing modifications
            // IsDataModified tells Il2CppInspector that the contents of its internal data structures have been changed
            // Note this is different from IsStreamModified; changing the data in memory
            // does not automatically rewrite the stream.
            data.IsDataModified = true;
        }

        public void OptionsChanged(PluginOptionsChangedEventInfo e)
        {
            Console.Write("Skipping OptionsChanged\n");
        }

        public void PostProcessMetadata(Metadata metadata, PluginPostProcessMetadataEventInfo data)
        {
            Console.Write("Skipping PostProcessMetadata\n");
        }

        public void PreProcessImage(BinaryObjectStream stream, PluginPreProcessImageEventInfo data)
        {
            Console.Write("Skipping PreProcessImage\n");
        }

        public void PostProcessImage<T>(FileFormatStream<T> stream, PluginPostProcessImageEventInfo data) where T : FileFormatStream<T>
        {
            Console.Write("Skipping PostProcessImage\n");
        }

        public void PreProcessBinary(Il2CppBinary binary, PluginPreProcessBinaryEventInfo data)
        {
            Console.Write("Skipping PreProcessBinary\n");
        }

        public void PostProcessBinary(Il2CppBinary binary, PluginPostProcessBinaryEventInfo data)
        {
            Console.Write("Skipping PostProcessBinary\n");
        }

        public void PostProcessTypeModel(TypeModel model, PluginPostProcessTypeModelEventInfo data)
        {
            Console.Write("Skipping PostProcessTypeModel\n");
        }

        public void PostProcessAppModel(AppModel appModel, PluginPostProcessAppModelEventInfo data)
        {
            Console.Write("Skipping PostProcessAppModel\n");
        }
    }
}
