using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SkyeTek
{
    namespace Tags
    {
        /// <summary>
        /// Supported tag types.
        /// </summary>
        public enum TagType : ushort 
        {
            AUTO_DETECT = 0x0000,
            ISO_15693_AUTO_DETECT = 0x0100,
            TI_15693_AUTO_DETECT = 0x0110,
            TAGIT_HF1_STANDARD = 0x0111,
            TAGIT_HF1_PRO = 0x0112,
            TAGIT_HF1_PLUS = 0x0113,
            PHILIPS_15693_AUTO_DETECT = 0x0120,
            ICODE_SLI_SL2 = 0x0121,
            ST_15693_AUTO_DETECT = 0x0130,
            ISO_LRI64 = 0x0131,
            LRI512 = 0x0132,
            LRI2K = 0x0133,
            LRIS2K = 0x0134,
            EM_15693_AUTO_DETECT = 0x0140,
            EM4006 = 0x0141,
            EM4034 = 0x0142,
            EM4035_CRYPTO = 0x0143,
            EM4135 = 0x0144,
            INFINEON_15693_AUTO_DETECT = 0x0150,
            MYD2K = 0x0151,
            MYD2KS = 0x0152,
            MYD10K = 0x0153,
            MYD10KS = 0x0154,
            FUJITSU_AUTO_DETECT = 0x0160,
            MB89R118 = 0x0161,
            ISO_14443A_AUTO_DETECT = 0x0200,
            TAGSYS_15693 = 0x0170,
            TAGSYS_C370 = 0x0171,
            PHILIPS_14443A_AUTO_DETECT = 0x0210,
            ISO_MIFARE_ULTRALIGHT = 0x0211,
            MIFARE_1K = 0x0212,
            MIFARE_4K = 0x0213,
            MIFARE_DESFIRE = 0x0214,
            MIFARE_PROX = 0x0215,
            INNOVISION_14443A_AUTO_DETECT = 0x0220,
            JEWEL = 0x0221,
            ISO_14443B_AUTO_DETECT = 0x0300,
            ATMEL_14443B_AUTO_DETECT = 0x0310,
            CRYPTORF_1K = 0x0311,
            CRYPTORF_2K = 0x0312,
            CRYPTORF_4K = 0x0313,
            CRYPTORF_8K = 0x0314,
            CRYPTORF_16K = 0x0315,
            CRYPTORF_32K = 0x0316,
            CRYPTORF_64K = 0x0317,
            AT88RF001 = 0x0318,
            AT88RF020 = 0x0319,
            SAMSUNG_14443B_AUTO_DETECT = 0x0330,
            S3C89K8 = 0x0331,
            S3C89V5 = 0x0332,
            S3C89V8 = 0x0333,
            S3CC9G4 = 0x0334,
            S3CC9GC = 0x0335,
            S3CC9GW = 0x0336,
            S3CC9W4 = 0x0337,
            S3CC9W9 = 0x0338,
            ST_MICRO_14443B_AUTO_DETECT = 0x0350,
            ST_MICRO_SRIX4K = 0x0351,
            ST_MICRO_SRI176 = 0x0352,
            ST_MICRO_SRI512 = 0x0353,
            AMEX_CARD = 0x0361,
            AMEX_FOB = 0x0362,
            ISO_18000_3_MODE1_AUTO_DETECT = 0x0400,
            M1_TI_15693_AUTO_DETECT = 0x0410,
            M1_TAGIT_HF1_STANDARD = 0x0411,
            M1_TAGIT_HF1_PRO = 0x0412,
            M1_TAGIT_HF1_PLUS = 0x0413,
            M1_PHILIPS_15693_AUTO_DETECT = 0x0420,
            M1_ICODE_SLI_SL2 = 0x0421,
            M1_ST_15693_AUTO_DETECT = 0x0430,
            M1_LRI64 = 0x0431,
            M1_LRI512 = 0x0432,
            M1_EM_15693_AUTO_DETECT = 0x0440,
            M1_EM4006 = 0x0441,
            M1_EM4034 = 0x0442,
            M1_EM4035_CRYPTO = 0x0443,
            M1_EM4135 = 0x0444,
            M1_INFINEON_15693_AUTO_DETECT = 0x0450,
            M1_MYD2K = 0x0451,
            M1_MYD2KS = 0x0452,
            M1_MYD10K = 0x0453,
            M1_MYD10KS = 0x0454,
            ISO_18000_3_MODE1_EXTENDED_AUTO_DETECT = 0x0500,
            RFU = 0x0510,
            TAGSYS = 0x0511,
            ISO_18000_3_MODE2_AUTO_DETECT = 0x0600,
            INFINEON_AUTO_DETECT = 0x0610,
            INFINEON_PJM_TAG = 0x0611,
            ISO_18092_AUTO_DETECT = 0x0700,
            ISO_21481_AUTO_DETECT = 0x0800,
            HF_PROPRIETARY_RFU = 0x0900,
            TAGIT_HF = 0x0901,
            ICODE1 = 0x0902,
            HF_EPC = 0x0903,
            LTO_PHILIPS = 0x0904,
            LTO_ATMEL = 0x0905,
            FELICIA = 0x0906,
            PICOTAG_2K = 0x0907,
            PICOTAG_16K = 0x0908,
            PICOTAG_2KS = 0x0909,
            PICOTAG_16KS = 0x0910,
            HID_ICLASS = 0x0911,
            GEMWARE_C210 = 0x0912,
            GEMWARE_C220 = 0x0913,
            GEMWARE_C240 = 0x0914,
            SR176 = 0x0915,
            SKYETEK_AFE = 0x0916,
            ICODE_UID_ICS11 = 0x0917,
            ICODE_UID_ICS12 = 0x0918,
            EPC_CLASS0_AUTO_DETECT = 0x8000,
            SYMBOL_CLASS0_AUTO_DETECT = 0x8010,
            MATRICS_CLASS0 = 0x8011,
            MATRICS_CLASS0_PLUS = 0x8012,
            IMPINJ_CLASS0_AUTO_DETECT = 0x8020,
            ZUMA = 0x8021,
            EPC_CLASS1_GEN1_AUTO_DETECT = 0x8100,
            ALIEN_C1G1_AUTO_DETECT = 0x8110,
            QUARK = 0x8111,
            OMEGA = 0x8112,
            LEPTON = 0x8113,
            ST_MICRO_C1G1_AUTO_DETECT = 0x8120,
            XRA00 = 0x8121,
            ISO_18000_6C_AUTO_DETECT = 0x8200, /* EPC Class1 Gen2 */
            IMPINJ_C1G2_AUTO_DETECT = 0x8210,
            MONZA = 0x8211,
            PHILIPS_C1G2_AUTO_DETECT = 0x8220,
            UCODE_EPC_G2 = 0x8221,
            MOTOROLA_EPC_G2 = 0x8222,
            NXP_G2XL = 0x8223,
            NXP_G2XM = 0x8224,
            ST_C1G2_AUTO_DETECT = 0x8230,
            XRAG2 = 0x8231,
            ALIEN_HIGGS = 0x8251,
            ALIEN_HIGGS_H3 = 0x8252,
 	        EM_C1G2_AUTO = 0x8260,
 	        EM_C1G2_EM4024 = 0x8261,
 	        EM_C1G2_EM4124 = 0x8262,
 	        EM_C1G2_EM4224 = 0x8263,
 	        EM_C1G2_EM4324 = 0x8264,
            HITACHI_HIBIKI_I = 0x8271,
            HITACHI_HIBIKI_II = 0x8272,
            ISO_18000_6B_AUTO_DETECT = 0x8300,
            PHILIPS_18000_6B_AUTO_DETECT = 0x8310,
            UCODE_1_19 = 0x8311,
            UCODE_HSL = 0x8312,
            FUJITSU_ISO180006B_AUTO_DETECT = 0x8320,
            FUJITSU_MB97R8010 = 0x8321,
            FUJITSU_MB97R8020 = 0x8322,
            ISO_18000_6A_AUTO_DETECT = 0x8400,
            EM_6A = 0x8401,
            EM_IPX_AUTO = 0x8500,
 	        EM4X22_AUTO = 0x8510,
 	        EM4022 = 0x8511,
 	        EM4122 = 0x8512,
 	        EM4222 = 0x8513,
 	        EM4X44_AUTO = 0x8520,
 	        EM4044 = 0x8521,
 	        EM4144 = 0x8522,
 	        EM4244 = 0x8523,
 	        EM4344 = 0x8524,
 	        EM4444 = 0x8525
        }

        public static class TagTypeDescriptions
        {
            public static Dictionary<TagType, string> descriptions = new Dictionary<TagType, string>();
            //public static TagType[] tagTypes = (TagType[])Enum.GetValues(typeof(TagType));

            static TagTypeDescriptions()
            {
                descriptions.Add(TagType.AUTO_DETECT, "Auto Detect");
                descriptions.Add(TagType.ISO_15693_AUTO_DETECT, "ISO15693 Auto Detect");
                descriptions.Add(TagType.TI_15693_AUTO_DETECT, "TI 15693 Auto Detect");
                descriptions.Add(TagType.TAGIT_HF1_STANDARD, "Tag-It HF-I Standard (2k bits)");
                descriptions.Add(TagType.TAGIT_HF1_PRO, "Tag-It HF-I Pro");
                descriptions.Add(TagType.TAGIT_HF1_PLUS, "Tag-It HF-I Plus");
                descriptions.Add(TagType.PHILIPS_15693_AUTO_DETECT, "Philips 15693 Auto Detect");
                descriptions.Add(TagType.ICODE_SLI_SL2, "I-Code SLI SL2 (1k bits)");
                descriptions.Add(TagType.ST_15693_AUTO_DETECT, "ST 15693 Auto Detect");
                descriptions.Add(TagType.ISO_LRI64, "LRI 64");
                descriptions.Add(TagType.LRI512, "LRI 512");
                descriptions.Add(TagType.LRI2K, "LRI 2K");
                descriptions.Add(TagType.LRIS2K, "LRIS 2K");
                descriptions.Add(TagType.EM_15693_AUTO_DETECT, "EM 15693 Auto Detect");
                descriptions.Add(TagType.EM4006, "EM4006");
                descriptions.Add(TagType.EM4034, "EM4034");
                descriptions.Add(TagType.EM4035_CRYPTO, "EM4035 (Crypto)");
                descriptions.Add(TagType.EM4135, "EM4135");
                descriptions.Add(TagType.INFINEON_15693_AUTO_DETECT, "Infineon 15693 Auto Detect");
                descriptions.Add(TagType.MYD2K, "My-D 2K");
                descriptions.Add(TagType.MYD2KS, "My-D 2KS");
                descriptions.Add(TagType.MYD10K, "My-D 10K");
                descriptions.Add(TagType.MYD10KS, "My-D 10KS");
                descriptions.Add(TagType.FUJITSU_AUTO_DETECT, "Fujitsu Auto Detect");
                descriptions.Add(TagType.MB89R118, "MB89R118");
                descriptions.Add(TagType.ISO_14443A_AUTO_DETECT, "ISO14443A Auto Detect");
                descriptions.Add(TagType.TAGSYS_15693, "TagSys 15693");
                descriptions.Add(TagType.TAGSYS_C370, "TagSys C370");
                descriptions.Add(TagType.PHILIPS_14443A_AUTO_DETECT, "Philips 14443A Auto Detect");
                descriptions.Add(TagType.ISO_MIFARE_ULTRALIGHT, "Mifare Ultralight");
                descriptions.Add(TagType.MIFARE_1K, "Mifare 1k");
                descriptions.Add(TagType.MIFARE_4K, "Mifare 4k");
                descriptions.Add(TagType.MIFARE_DESFIRE, "Mifare DESfire");
                descriptions.Add(TagType.MIFARE_PROX, "Mifare Pro X");
                descriptions.Add(TagType.INNOVISION_14443A_AUTO_DETECT, "Innovision 14443A Auto Detect");
                descriptions.Add(TagType.JEWEL, "Jewel");
                descriptions.Add(TagType.ISO_14443B_AUTO_DETECT, "ISO14443B Auto Detect");
                descriptions.Add(TagType.ATMEL_14443B_AUTO_DETECT, "Atmel 14443B Auto Detect");
                descriptions.Add(TagType.CRYPTORF_1K, "CryptoRF (1k bits)");
                descriptions.Add(TagType.CRYPTORF_2K, "CryptoRF (2k bits)");
                descriptions.Add(TagType.CRYPTORF_4K, "CryptoRF (4k bits)");
                descriptions.Add(TagType.CRYPTORF_8K, "CryptoRF (8k bits)");
                descriptions.Add(TagType.CRYPTORF_16K, "CryptoRF (16k bits)");
                descriptions.Add(TagType.CRYPTORF_32K, "CryptoRF (32k bits)");
                descriptions.Add(TagType.CRYPTORF_64K, "CryptoRF (64k bits)");
                descriptions.Add(TagType.AT88RF001, "AT88RF001 (256 bits)");
                descriptions.Add(TagType.AT88RF020, "AT88RF020");
                descriptions.Add(TagType.SAMSUNG_14443B_AUTO_DETECT, "Samsung 14443B Auto Detect");
                descriptions.Add(TagType.S3C89K8, "S3C89K8 (8k bits)");
                descriptions.Add(TagType.S3C89V5, "S3C89V5 (16k bits)");
                descriptions.Add(TagType.S3C89V8, "S3C89V8 (8192)");
                descriptions.Add(TagType.S3CC9G4, "S3CC9G4 (4096)");
                descriptions.Add(TagType.S3CC9GC, "S3CC9GC (72kB)");
                descriptions.Add(TagType.S3CC9GW, "S3CC9GW (144 kB)");
                descriptions.Add(TagType.S3CC9W4, "S3CC9W4 (4 kB)");
                descriptions.Add(TagType.S3CC9W9, "S3CC9W9 (32 kB)");
                descriptions.Add(TagType.ST_MICRO_14443B_AUTO_DETECT, "ST Micro 14443B Auto Detect");
                descriptions.Add(TagType.AMEX_CARD, "AMEX Card");
                descriptions.Add(TagType.AMEX_FOB, "AMEX FOB");
                descriptions.Add(TagType.ISO_18000_3_MODE1_AUTO_DETECT, "ISO18000-3 Mode 1 Auto Detect");
                descriptions.Add(TagType.M1_TI_15693_AUTO_DETECT, "TI 15693 Auto Detect (M1)");
                descriptions.Add(TagType.M1_TAGIT_HF1_STANDARD, "Tag-It HF-I Standard (2k bits) (M1)");
                descriptions.Add(TagType.M1_TAGIT_HF1_PRO, "Tag-It HF-I Pro (M1)");
                descriptions.Add(TagType.M1_TAGIT_HF1_PLUS, "Tag-It HF-I Plus (M1)");
                descriptions.Add(TagType.M1_PHILIPS_15693_AUTO_DETECT, "Philips 15693 Auto Detect (M1)");
                descriptions.Add(TagType.M1_ICODE_SLI_SL2, "I-Code SLI SL2 (1k bits) (M1)");
                descriptions.Add(TagType.M1_ST_15693_AUTO_DETECT, "ST 15693 Auto Detect (M1)");
                descriptions.Add(TagType.M1_LRI64, "LRI 64 (M1)");
                descriptions.Add(TagType.M1_LRI512, "LRI 512 (M1)");
                descriptions.Add(TagType.M1_EM_15693_AUTO_DETECT, "EM 15693 Auto Detect (M1)");
                descriptions.Add(TagType.M1_EM4006, "EM4006 (M1)");
                descriptions.Add(TagType.M1_EM4034, "EM4034 (M1)");
                descriptions.Add(TagType.M1_EM4035_CRYPTO, "EM4035 (Crypto) (M1)");
                descriptions.Add(TagType.M1_EM4135, "EM4135 (M1)");
                descriptions.Add(TagType.M1_INFINEON_15693_AUTO_DETECT, "Infineon 15693 Auto Detect (M1)");
                descriptions.Add(TagType.M1_MYD2K, "My-D 2K (M1)");
                descriptions.Add(TagType.M1_MYD2KS, "My-D 2KS (M1)");
                descriptions.Add(TagType.M1_MYD10K, "My-D 10K (M1)");
                descriptions.Add(TagType.M1_MYD10KS, "My-D 10KS (M1)");
                descriptions.Add(TagType.ISO_18000_3_MODE1_EXTENDED_AUTO_DETECT, "ISO18000-3 Mode 1 Extended Auto Detect");
                descriptions.Add(TagType.RFU, "RFU");
                descriptions.Add(TagType.TAGSYS, "TagSys");
                descriptions.Add(TagType.ISO_18000_3_MODE2_AUTO_DETECT, "ISO18000-3 Mode 2 Auto Detect");
                descriptions.Add(TagType.INFINEON_AUTO_DETECT, "Infineon Auto Detect");
                descriptions.Add(TagType.INFINEON_PJM_TAG, "Infineon PJM Tag");
                descriptions.Add(TagType.ISO_18092_AUTO_DETECT, "ISO18092 Auto Detect");
                descriptions.Add(TagType.ISO_21481_AUTO_DETECT, "ISO21481 Auto Detect");
                descriptions.Add(TagType.SKYETEK_AFE, "SkyeTek Crypto API");
                descriptions.Add(TagType.HF_PROPRIETARY_RFU, "HF Proprietary RFU");
                descriptions.Add(TagType.TAGIT_HF, "Tag-It HF");
                descriptions.Add(TagType.ICODE1, "I-Code1");
                descriptions.Add(TagType.HF_EPC, "HF EPC");
                descriptions.Add(TagType.LTO_PHILIPS, "LTO - Philips");
                descriptions.Add(TagType.LTO_ATMEL, "LTO - Atmel");
                descriptions.Add(TagType.FELICIA, "FeliCA");
                descriptions.Add(TagType.PICOTAG_2K, "PicoTag 2k");
                descriptions.Add(TagType.PICOTAG_16K, "PicoTag 16k");
                descriptions.Add(TagType.PICOTAG_2KS, "PicoTag 2kS");
                descriptions.Add(TagType.PICOTAG_16KS, "PicoTag 16kS");
                descriptions.Add(TagType.HID_ICLASS, "HID I-Class");
                descriptions.Add(TagType.GEMWARE_C210, "GemWave C210");
                descriptions.Add(TagType.GEMWARE_C220, "GemWave C220");
                descriptions.Add(TagType.GEMWARE_C240, "GemWave C240");
                descriptions.Add(TagType.SR176, "SR176");
                descriptions.Add(TagType.ICODE_UID_ICS11, "I-Code UID ICS 11");
                descriptions.Add(TagType.ICODE_UID_ICS12, "I-Code UID ICS 12");
                descriptions.Add(TagType.EPC_CLASS0_AUTO_DETECT, "EPC Class 0 Auto Detect");
                descriptions.Add(TagType.SYMBOL_CLASS0_AUTO_DETECT, "Symbol Class 0 Auto Detect");
                descriptions.Add(TagType.MATRICS_CLASS0, "Matrics Class 0  (96 bits)");
                descriptions.Add(TagType.MATRICS_CLASS0_PLUS, "Matrics Class 0+  (256 bits)");
                descriptions.Add(TagType.IMPINJ_CLASS0_AUTO_DETECT, "Impinj Class 0 Auto Detect");
                descriptions.Add(TagType.ZUMA, "Zuma (256 bits) (Class 0+)");
                descriptions.Add(TagType.EPC_CLASS1_GEN1_AUTO_DETECT, "EPC Class 1 Gen 1 Auto Detect");
                descriptions.Add(TagType.ALIEN_C1G1_AUTO_DETECT, "Alien C1G1 Auto Detect");
                descriptions.Add(TagType.QUARK, "Quark (64 bits)");
                descriptions.Add(TagType.OMEGA, "Omega (64 bits)");
                descriptions.Add(TagType.LEPTON, "Lepton (96 bits)");
                descriptions.Add(TagType.ST_MICRO_C1G1_AUTO_DETECT, "ST Micro C1G1 Auto Detect");
                descriptions.Add(TagType.XRA00, "XRA00 (64 bits)");
                descriptions.Add(TagType.ISO_18000_6C_AUTO_DETECT, "ISO18000-6C (EPC Class 1 Gen 2) Auto Detect");
                descriptions.Add(TagType.IMPINJ_C1G2_AUTO_DETECT, "Impinj C1G2 Auto Detect");
                descriptions.Add(TagType.MONZA, "Monza (256 bits)");
                descriptions.Add(TagType.PHILIPS_C1G2_AUTO_DETECT, "Philips C1G2 Auto Detect");
                descriptions.Add(TagType.UCODE_EPC_G2, "UCode EPC G2 (512 bits)");
                descriptions.Add(TagType.MOTOROLA_EPC_G2, "Motorola EPC Gen2");
                descriptions.Add(TagType.NXP_G2XL, "NXP G2XL (EPC Class1 Gen2)");
                descriptions.Add(TagType.NXP_G2XM, "NXP G2XM (EPC Class1 Gen2)"); 
                descriptions.Add(TagType.ST_C1G2_AUTO_DETECT, "ST C1G2 Auto Detect");
                descriptions.Add(TagType.XRAG2, "XRAG2 (432 bits)");
                descriptions.Add(TagType.ALIEN_HIGGS, "Alien Higgs");
                descriptions.Add(TagType.ALIEN_HIGGS_H3, "Alien Higgs 3 (H3)");
                descriptions.Add(TagType.EM_C1G2_AUTO, "EM C1G2 Auto Detect");
                descriptions.Add(TagType.EM_C1G2_EM4024, "EM4024 C1G2");
                descriptions.Add(TagType.EM_C1G2_EM4124, "EM4124 C1G2");
                descriptions.Add(TagType.EM_C1G2_EM4224, "EM4224 C1G2");
                descriptions.Add(TagType.EM_C1G2_EM4324, "EM4324 C1G2");
                descriptions.Add(TagType.HITACHI_HIBIKI_I, "Hitachi Hibiki");
                descriptions.Add(TagType.HITACHI_HIBIKI_II, "Hitachi Hibiki II");
                descriptions.Add(TagType.ISO_18000_6B_AUTO_DETECT, "ISO18000-6B Auto Detect");
                descriptions.Add(TagType.PHILIPS_18000_6B_AUTO_DETECT, "Philips 18000-6B Auto Detect");
                descriptions.Add(TagType.UCODE_1_19, "UCode EPC 1.19 (256 bits)");
                descriptions.Add(TagType.UCODE_HSL, "UCode HSL (1728 bits)");
                descriptions.Add(TagType.FUJITSU_ISO180006B_AUTO_DETECT, "Fujitsu ISO180006B Auto Detect");
                descriptions.Add(TagType.FUJITSU_MB97R8010, "Fujitsu MB97R8010");
                descriptions.Add(TagType.FUJITSU_MB97R8020, "Fujitsu MB97R8020");
                descriptions.Add(TagType.ISO_18000_6A_AUTO_DETECT, "ISO18000-6A Auto Detect");
                descriptions.Add(TagType.EM_6A, "EM 6A Tag");
                descriptions.Add(TagType.EM_IPX_AUTO, "EM IPX Auto Detect");
                descriptions.Add(TagType.EM4X22_AUTO, "EM4X22 Auto Detect");
                descriptions.Add(TagType.EM4022, "EM4022");
                descriptions.Add(TagType.EM4122, "EM4122");
                descriptions.Add(TagType.EM4222, "EM4222");
                descriptions.Add(TagType.EM4X44_AUTO, "EM4X44 Auto Detect");
                descriptions.Add(TagType.EM4044, "EM4044");
                descriptions.Add(TagType.EM4144, "EM4144");
                descriptions.Add(TagType.EM4244, "EM4244");
                descriptions.Add(TagType.EM4344, "EM4344");
                descriptions.Add(TagType.EM4444, "EM4444");
            }

        }

        public class Tag
        {
            protected TagType m_type;
            protected byte[] m_tid;

            public Tag()
            {
                this.m_tid = null;
                this.m_type = TagType.AUTO_DETECT;
            }

            /// <summary>
            /// Type for this tag. Must be one of the members
            /// of <see cref="TagType"/>
            /// </summary>
            public TagType Type
            {
                get { return this.m_type; }
                set { this.m_type = value; }
            }

            /// <summary>
            /// Tag ID for this tag.  Variable length with maximum of 16 bytes.
            /// </summary>
            public byte[] TID
            {
                get { return this.m_tid; }
                set { this.m_tid = value; }
            }
        }
    }
}