//
// Author: Denis Potapenko
// http://denis-potapenko.blogspot.com/
// 
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

//typedef unsigned int   ILenum;
//typedef unsigned char  ILboolean;
//typedef unsigned int   ILbitfield;
//typedef signed char    ILbyte;
//typedef signed short   ILshort;
//typedef int     	   ILint;
//typedef size_t         ILsizei;
//typedef unsigned char  ILubyte;
//typedef unsigned short ILushort;
//typedef unsigned int   ILuint;
//typedef float          ILfloat;
//typedef float          ILclampf;
//typedef double         ILdouble;
//typedef double         ILclampd; 

public class DevILLoader
{
    [DllImport("DevIL")]
    public static extern void ilInit();

    [DllImport("DevIL")]
    public static extern bool ilEnable(uint mode);

    [DllImport("DevIL")]
    public static extern void ilGenImages(uint num, uint[] images);

    [DllImport("DevIL")]
    public static extern void ilBindImage(uint image);

    [DllImport("DevIL")]
    public static extern bool ilLoadImage(string fileName);

    [DllImport("DevIL")]
    public static extern bool ilLoadL(uint type, byte[] lump, uint size);

    [DllImport("DevIL")]
    public static extern int ilGetInteger(uint mode);

    [DllImport("DevIL")]
    public static extern uint ilCopyPixels(uint xoff, uint yoff, uint zoff, uint width, uint height, uint depth, uint format, uint type, byte[] data);
    
    [DllImport("DevIL")]
    public static extern void ilDeleteImages(uint num, uint[] images);
    
    [DllImport("DevIL")]
    public static extern bool ilActiveMipmap(uint number);    

  
}

///////////////////////////////////////////////////////////////////////////////
#region DevILConstants
public class DevILConstants
{
    // don't use it! Use simple bool
    //public const uint IL_FALSE = 0;
    //public const uint IL_TRUE = 1;

    //  Matches OpenGL's right now.
    //! Data formats \link Formats Formats\endlink
    public const uint IL_COLOUR_INDEX = 0x1900;
    public const uint IL_COLOR_INDEX = 0x1900;
    public const uint IL_ALPHA = 0x1906;
    public const uint IL_RGB = 0x1907;
    public const uint IL_RGBA = 0x1908;
    public const uint IL_BGR = 0x80E0;
    public const uint IL_BGRA = 0x80E1;
    public const uint IL_LUMINANCE = 0x1909;
    public const uint IL_LUMINANCE_ALPHA = 0x190A;

    //! Data types \link Types Types\endlink
    public const uint IL_BYTE = 0x1400;
    public const uint IL_UNSIGNED_BYTE = 0x1401;
    public const uint IL_SHORT = 0x1402;
    public const uint IL_UNSIGNED_SHORT = 0x1403;
    public const uint IL_INT = 0x1404;
    public const uint IL_UNSIGNED_INT = 0x1405;
    public const uint IL_FLOAT = 0x1406;
    public const uint IL_DOUBLE = 0x140A;
    public const uint IL_HALF = 0x140B;


    //public const uint IL_MAX_BYTE		  		SCHAR_MAX
    //public const uint IL_MAX_UNSIGNED_BYTE  	UCHAR_MAX
    //public const uint IL_MAX_SHORT	  		SHRT_MAX
    //public const uint IL_MAX_UNSIGNED_SHORT 	USHRT_MAX
    //public const uint IL_MAX_INT		  		INT_MAX
    //public const uint IL_MAX_UNSIGNED_INT   	UINT_MAX

    //public const uint IL_LIMIT(x,m,M)		(x<m?m:(x>M?M:x))
    //public const uint IL_CLAMP(x) 		IL_LIMIT(x,0,1)

    public const uint IL_VENDOR = 0x1F00;
    public const uint IL_LOAD_EXT = 0x1F01;
    public const uint IL_SAVE_EXT = 0x1F02;


    //
    // IL-specific #define's
    //

    //public const uint IL_VERSION_1_7_8 1
    //public const uint IL_VERSION       178


    // Attribute Bits
    public const uint IL_ORIGIN_BIT = 0x00000001;
    public const uint IL_FILE_BIT = 0x00000002;
    public const uint IL_PAL_BIT = 0x00000004;
    public const uint IL_FORMAT_BIT = 0x00000008;
    public const uint IL_TYPE_BIT = 0x00000010;
    public const uint IL_COMPRESS_BIT = 0x00000020;
    public const uint IL_LOADFAIL_BIT = 0x00000040;
    public const uint IL_FORMAT_SPECIFIC_BIT = 0x00000080;
    public const uint IL_ALL_ATTRIB_BITS = 0x000FFFFF;


    // Palette types
    public const uint IL_PAL_NONE = 0x0400;
    public const uint IL_PAL_RGB24 = 0x0401;
    public const uint IL_PAL_RGB32 = 0x0402;
    public const uint IL_PAL_RGBA32 = 0x0403;
    public const uint IL_PAL_BGR24 = 0x0404;
    public const uint IL_PAL_BGR32 = 0x0405;
    public const uint IL_PAL_BGRA32 = 0x0406;


    // Image types
    public const uint IL_TYPE_UNKNOWN = 0x0000;
    public const uint IL_BMP = 0x0420; //!< Microsoft Windows Bitmap - .bmp extension
    public const uint IL_CUT = 0x0421;  //!< Dr. Halo - .cut extension
    public const uint IL_DOOM = 0x0422;  //!< DooM walls - no specific extension
    public const uint IL_DOOM_FLAT = 0x0423;  //!< DooM flats - no specific extension
    public const uint IL_ICO = 0x0424;  //!< Microsoft Windows Icons and Cursors - .ico and .cur extensions
    public const uint IL_JPG = 0x0425;  //!< JPEG - .jpg, .jpe and .jpeg extensions
    public const uint IL_JFIF = 0x0425;  //!<
    public const uint IL_ILBM = 0x0426;  //!< Amiga IFF (FORM ILBM) - .iff, .ilbm, .lbm extensions
    public const uint IL_PCD = 0x0427;  //!< Kodak PhotoCD - .pcd extension
    public const uint IL_PCX = 0x0428;  //!< ZSoft PCX - .pcx extension
    public const uint IL_PIC = 0x0429;  //!< PIC - .pic extension
    public const uint IL_PNG = 0x042A;  //!< Portable Network Graphics - .png extension
    public const uint IL_PNM = 0x042B;  //!< Portable Any Map - .pbm, .pgm, .ppm and .pnm extensions
    public const uint IL_SGI = 0x042C;  //!< Silicon Graphics - .sgi, .bw, .rgb and .rgba extensions
    public const uint IL_TGA = 0x042D;  //!< TrueVision Targa File - .tga, .vda, .icb and .vst extensions
    public const uint IL_TIF = 0x042E;  //!< Tagged Image File Format - .tif and .tiff extensions
    public const uint IL_CHEAD = 0x042F;  //!< C-Style Header - .h extension
    public const uint IL_RAW = 0x0430;  //!< Raw Image Data - any extension
    public const uint IL_MDL = 0x0431;  //!< Half-Life Model Texture - .mdl extension
    public const uint IL_WAL = 0x0432;  //!< Quake 2 Texture - .wal extension
    public const uint IL_LIF = 0x0434;  //!< Homeworld Texture - .lif extension
    public const uint IL_MNG = 0x0435;  //!< Multiple-image Network Graphics - .mng extension
    public const uint IL_JNG = 0x0435;  //!< 
    public const uint IL_GIF = 0x0436;  //!< Graphics uinterchange Format - .gif extension
    public const uint IL_DDS = 0x0437;  //!< DirectDraw Surface - .dds extension
    public const uint IL_DCX = 0x0438;  //!< ZSoft Multi-PCX - .dcx extension
    public const uint IL_PSD = 0x0439;  //!< Adobe PhotoShop - .psd extension
    public const uint IL_EXIF = 0x043A;  //!< 
    public const uint IL_PSP = 0x043B;  //!< PaintShop Pro - .psp extension
    public const uint IL_PIX = 0x043C;  //!< PIX - .pix extension
    public const uint IL_PXR = 0x043D;  //!< Pixar - .pxr extension
    public const uint IL_XPM = 0x043E;  //!< X Pixel Map - .xpm extension
    public const uint IL_HDR = 0x043F;  //!< Radiance High Dynamic Range - .hdr extension
    public const uint IL_ICNS = 0x0440;  //!< Macintosh Icon - .icns extension
    public const uint IL_JP2 = 0x0441;  //!< Jpeg 2000 - .jp2 extension
    public const uint IL_EXR = 0x0442;  //!< OpenEXR - .exr extension
    public const uint IL_WDP = 0x0443;  //!< Microsoft HD Photo - .wdp and .hdp extension
    public const uint IL_VTF = 0x0444;  //!< Valve Texture Format - .vtf extension
    public const uint IL_WBMP = 0x0445;  //!< Wireless Bitmap - .wbmp extension
    public const uint IL_SUN = 0x0446;  //!< Sun Raster - .sun, .ras, .rs, .im1, .im8, .im24 and .im32 extensions
    public const uint IL_IFF = 0x0447;  //!< uinterchange File Format - .iff extension
    public const uint IL_TPL = 0x0448;  //!< Gamecube Texture - .tpl extension
    public const uint IL_FITS = 0x0449;  //!< Flexible Image Transport System - .fit and .fits extensions
    public const uint IL_DICOM = 0x044A;  //;!< Digital Imaging and Communications in Medicine (DICOM) - .dcm and .dicom extensions
    public const uint IL_IWI = 0x044B;  //!< Call of Duty Infinity Ward Image - .iwi extension
    public const uint IL_BLP = 0x044C;  //!< Blizzard Texture Format - .blp extension
    public const uint IL_FTX = 0x044D;  //!< Heavy Metal: FAKK2 Texture - .ftx extension
    public const uint IL_ROT = 0x044E;  //!< Homeworld 2 - Relic Texture - .rot extension
    public const uint IL_TEXTURE = 0x044F;  //!< Medieval II: Total War Texture - .texture extension
    public const uint IL_DPX = 0x0450;  //!< Digital Picture Exchange - .dpx extension
    public const uint IL_UTX = 0x0451;  //!< Unreal (and Unreal Tournament) Texture - .utx extension
    public const uint IL_MP3 = 0x0452;  //!< MPEG-1 Audio Layer 3 - .mp3 extension
    public const uint IL_JASC_PAL = 0x0475;  //!< PaintShop Pro Palette


    // Error Types
    public const uint IL_NO_ERROR = 0x0000;
    public const uint IL_INVALID_ENUM = 0x0501;
    public const uint IL_OUT_OF_MEMORY = 0x0502;
    public const uint IL_FORMAT_NOT_SUPPORTED = 0x0503;
    public const uint IL_INTERNAL_ERROR = 0x0504;
    public const uint IL_INVALID_VALUE = 0x0505;
    public const uint IL_ILLEGAL_OPERATION = 0x0506;
    public const uint IL_ILLEGAL_FILE_VALUE = 0x0507;
    public const uint IL_INVALID_FILE_HEADER = 0x0508;
    public const uint IL_INVALID_PARAM = 0x0509;
    public const uint IL_COULD_NOT_OPEN_FILE = 0x050A;
    public const uint IL_INVALID_EXTENSION = 0x050B;
    public const uint IL_FILE_ALREADY_EXISTS = 0x050C;
    public const uint IL_OUT_FORMAT_SAME = 0x050D;
    public const uint IL_STACK_OVERFLOW = 0x050E;
    public const uint IL_STACK_UNDERFLOW = 0x050F;
    public const uint IL_INVALID_CONVERSION = 0x0510;
    public const uint IL_BAD_DIMENSIONS = 0x0511;
    public const uint IL_FILE_READ_ERROR = 0x0512; // 05/12/2002: Addition by Sam.
    public const uint IL_FILE_WRITE_ERROR = 0x0512;

    public const uint IL_LIB_GIF_ERROR = 0x05E1;
    public const uint IL_LIB_JPEG_ERROR = 0x05E2;
    public const uint IL_LIB_PNG_ERROR = 0x05E3;
    public const uint IL_LIB_TIFF_ERROR = 0x05E4;
    public const uint IL_LIB_MNG_ERROR = 0x05E5;
    public const uint IL_LIB_JP2_ERROR = 0x05E6;
    public const uint IL_LIB_EXR_ERROR = 0x05E7;
    public const uint IL_UNKNOWN_ERROR = 0x05FF;


    // Origin Definitions
    public const uint IL_ORIGIN_SET = 0x0600;
    public const uint IL_ORIGIN_LOWER_LEFT = 0x0601;
    public const uint IL_ORIGIN_UPPER_LEFT = 0x0602;
    public const uint IL_ORIGIN_MODE = 0x0603;


    // Format and Type Mode Definitions
    public const uint IL_FORMAT_SET = 0x0610;
    public const uint IL_FORMAT_MODE = 0x0611;
    public const uint IL_TYPE_SET = 0x0612;
    public const uint IL_TYPE_MODE = 0x0613;


    // File definitions
    public const uint IL_FILE_OVERWRITE = 0x0620;
    public const uint IL_FILE_MODE = 0x0621;


    // Palette definitions
    public const uint IL_CONV_PAL = 0x0630;


    // Load fail definitions
    public const uint IL_DEFAULT_ON_FAIL = 0x0632;


    // Key colour and alpha definitions
    public const uint IL_USE_KEY_COLOUR = 0x0635;
    public const uint IL_USE_KEY_COLOR = 0x0635;
    public const uint IL_BLIT_BLEND = 0x0636;


    // uinterlace definitions
    public const uint IL_SAVE_INTERLACED = 0x0639;
    public const uint IL_INTERLACE_MODE = 0x063A;


    // Quantization definitions
    public const uint IL_QUANTIZATION_MODE = 0x0640;
    public const uint IL_WU_QUANT = 0x0641;
    public const uint IL_NEU_QUANT = 0x0642;
    public const uint IL_NEU_QUANT_SAMPLE = 0x0643;
    public const uint IL_MAX_QUANT_INDEXS = 0x0644; //XIX : ILint : Maximum number of colors to reduce to, default of 256. and has a range of 2-256
    public const uint IL_MAX_QUANT_INDICES = 0x0644; // Redefined, since the above public const uint is misspelled


    // Hints
    public const uint IL_FASTEST = 0x0660;
    public const uint IL_LESS_MEM = 0x0661;
    public const uint IL_DONT_CARE = 0x0662;
    public const uint IL_MEM_SPEED_HINT = 0x0665;
    public const uint IL_USE_COMPRESSION = 0x0666;
    public const uint IL_NO_COMPRESSION = 0x0667;
    public const uint IL_COMPRESSION_HINT = 0x0668;


    // Compression
    public const uint IL_NVIDIA_COMPRESS = 0x0670;
    public const uint IL_SQUISH_COMPRESS = 0x0671;


    // Subimage types
    public const uint IL_SUB_NEXT = 0x0680;
    public const uint IL_SUB_MIPMAP = 0x0681;
    public const uint IL_SUB_LAYER = 0x0682;


    // Compression definitions
    public const uint IL_COMPRESS_MODE = 0x0700;
    public const uint IL_COMPRESS_NONE = 0x0701;
    public const uint IL_COMPRESS_RLE = 0x0702;
    public const uint IL_COMPRESS_LZO = 0x0703;
    public const uint IL_COMPRESS_ZLIB = 0x0704;


    // File format-specific values
    public const uint IL_TGA_CREATE_STAMP = 0x0710;
    public const uint IL_JPG_QUALITY = 0x0711;
    public const uint IL_PNG_INTERLACE = 0x0712;
    public const uint IL_TGA_RLE = 0x0713;
    public const uint IL_BMP_RLE = 0x0714;
    public const uint IL_SGI_RLE = 0x0715;
    public const uint IL_TGA_ID_STRING = 0x0717;
    public const uint IL_TGA_AUTHNAME_STRING = 0x0718;
    public const uint IL_TGA_AUTHCOMMENT_STRING = 0x0719;
    public const uint IL_PNG_AUTHNAME_STRING = 0x071A;
    public const uint IL_PNG_TITLE_STRING = 0x071B;
    public const uint IL_PNG_DESCRIPTION_STRING = 0x071C;
    public const uint IL_TIF_DESCRIPTION_STRING = 0x071D;
    public const uint IL_TIF_HOSTCOMPUTER_STRING = 0x071E;
    public const uint IL_TIF_DOCUMENTNAME_STRING = 0x071F;
    public const uint IL_TIF_AUTHNAME_STRING = 0x0720;
    public const uint IL_JPG_SAVE_FORMAT = 0x0721;
    public const uint IL_CHEAD_HEADER_STRING = 0x0722;
    public const uint IL_PCD_PICNUM = 0x0723;
    public const uint IL_PNG_ALPHA_INDEX = 0x0724; //XIX :; ILint : the color in the palette at this index value (0-255) is considered transparent, -1 for no trasparent color
    public const uint IL_JPG_PROGRESSIVE = 0x0725;
    public const uint IL_VTF_COMP = 0x0726;


    // DXTC definitions
    public const uint IL_DXTC_FORMAT = 0x0705;
    public const uint IL_DXT1 = 0x0706;
    public const uint IL_DXT2 = 0x0707;
    public const uint IL_DXT3 = 0x0708;
    public const uint IL_DXT4 = 0x0709;
    public const uint IL_DXT5 = 0x070A;
    public const uint IL_DXT_NO_COMP = 0x070B;
    public const uint IL_KEEP_DXTC_DATA = 0x070C;
    public const uint IL_DXTC_DATA_FORMAT = 0x070D;
    public const uint IL_3DC = 0x070E;
    public const uint IL_RXGB = 0x070F;
    public const uint IL_ATI1N = 0x0710;
    public const uint IL_DXT1A = 0x0711;  // Normally the same as IL_DXT1, except for nVidia Texture Tools.

    // Environment map definitions
    public const uint IL_CUBEMAP_POSITIVEX = 0x00000400;
    public const uint IL_CUBEMAP_NEGATIVEX = 0x00000800;
    public const uint IL_CUBEMAP_POSITIVEY = 0x00001000;
    public const uint IL_CUBEMAP_NEGATIVEY = 0x00002000;
    public const uint IL_CUBEMAP_POSITIVEZ = 0x00004000;
    public const uint IL_CUBEMAP_NEGATIVEZ = 0x00008000;
    public const uint IL_SPHEREMAP = 0x00010000;


    // Values
    public const uint IL_VERSION_NUM = 0x0DE2;
    public const uint IL_IMAGE_WIDTH = 0x0DE4;
    public const uint IL_IMAGE_HEIGHT = 0x0DE5;
    public const uint IL_IMAGE_DEPTH = 0x0DE6;
    public const uint IL_IMAGE_SIZE_OF_DATA = 0x0DE7;
    public const uint IL_IMAGE_BPP = 0x0DE8;
    public const uint IL_IMAGE_BYTES_PER_PIXEL = 0x0DE8;
    //public const uint IL_IMAGE_BPP = 0x0DE8;
    public const uint IL_IMAGE_BITS_PER_PIXEL = 0x0DE9;
    public const uint IL_IMAGE_FORMAT = 0x0DEA;
    public const uint IL_IMAGE_TYPE = 0x0DEB;
    public const uint IL_PALETTE_TYPE = 0x0DEC;
    public const uint IL_PALETTE_SIZE = 0x0DED;
    public const uint IL_PALETTE_BPP = 0x0DEE;
    public const uint IL_PALETTE_NUM_COLS = 0x0DEF;
    public const uint IL_PALETTE_BASE_TYPE = 0x0DF0;
    public const uint IL_NUM_FACES = 0x0DE1;
    public const uint IL_NUM_IMAGES = 0x0DF1;
    public const uint IL_NUM_MIPMAPS = 0x0DF2;
    public const uint IL_NUM_LAYERS = 0x0DF3;
    public const uint IL_ACTIVE_IMAGE = 0x0DF4;
    public const uint IL_ACTIVE_MIPMAP = 0x0DF5;
    public const uint IL_ACTIVE_LAYER = 0x0DF6;
    public const uint IL_ACTIVE_FACE = 0x0E00;
    public const uint IL_CUR_IMAGE = 0x0DF7;
    public const uint IL_IMAGE_DURATION = 0x0DF8;
    public const uint IL_IMAGE_PLANESIZE = 0x0DF9;
    public const uint IL_IMAGE_BPC = 0x0DFA;
    public const uint IL_IMAGE_OFFX = 0x0DFB;
    public const uint IL_IMAGE_OFFY = 0x0DFC;
    public const uint IL_IMAGE_CUBEFLAGS = 0x0DFD;
    public const uint IL_IMAGE_ORIGIN = 0x0DFE;
    public const uint IL_IMAGE_CHANNELS = 0x0DFF;
}
#endregion
///////////////////////////////////////////////////////////////////////////////
