using System;
using System.Collections.Generic;
using System.Linq;

namespace acnhpoker
{
    static class ItemAttr
    {
        public const UInt16 empty = 0xFFFE;
        private static readonly HashSet<UInt16> hasDurabilitySet = new HashSet<UInt16>
        {
            0x0833, // shovel
            0x0834, // axe
            0x0948, // flimsy net
            0x0949, // flimsy rod
            0x094b, // watering can
            0x0b06, // slingshot
            0x0bfc, // flimsy axe
            0x0c0a, // flimsy shovel
            0x0c0b, // flimsy watering can
            0x0c0c, // flimsy rod
            0x1698, // net
            0x16e0, // stone axe
            0x1ff0, // star net
            0x1ff1, // colorfulnet
            0x1ff2, // outdoorsy net
            0x1ff3, // golden net
            0x2117, // fish fishing rod
            0x2118, // colorful fishing rod
            0x2119, // outdoorsy fishing rod
            0x2155, // golden watering can
            0x2156, // colorful watering can
            0x2157, // elephant watering can
            0x2158, // outdoorsy watering can
            0x217e, // golden shovel
            0x217f, // outdoorsy shovel
            0x2180, // printed-design shovel
            0x2181, // colorful shovel
            0x2182, // golden slingshot
            0x2183, // outdoorsy slingshot
            0x2184, // colorful slingshot
            0x21d4, // golden rod
            0x2591, // golden axe
            0x3147, // worn axe
        };
        private static readonly HashSet<UInt16> hasGeneticsSet = new HashSet<UInt16>
        {
            0x0A43, // red-cosmos stems
            0x0A44, // red-cosmos buds
            0x0A46, // white-cosmos stems
            0x0A47, // white-cosmos buds
            0x0B31, // yellow-cosmos stems
            0x0B32, // yellow-cosmos buds
            0x0B35, // pink-cosmos stems
            0x0B36, // pink-cosmos buds
            0x0B39, // orange-cosmos stems
            0x0B3A, // orange-cosmos buds
            0x0B3D, // black-cosmos stems
            0x0B3E, // black-cosmos buds
            0x0B41, // white-tulip stems
            0x0B42, // white-tulip buds
            0x0B45, // red-tulip stems
            0x0B46, // red-tulip buds
            0x0B49, // yellow-tulip stems
            0x0B4A, // yellow-tulip buds
            0x0B4D, // pink-tulip stems
            0x0B4E, // pink-tulip buds
            0x0B51, // orange-tulip stems
            0x0B52, // orange-tulip buds
            0x0B55, // purple-tulip stems
            0x0B56, // purple-tulip buds
            0x0B59, // black-tulip stems
            0x0B5A, // black-tulip buds
            0x0B5D, // white-pansy stems
            0x0B5E, // white-pansy buds
            0x0B61, // red-pansy stems
            0x0B62, // red-pansy buds
            0x0B65, // yellow-pansy stems
            0x0B66, // yellow-pansy buds
            0x0B69, // orange-pansy stems
            0x0B6A, // orange-pansy buds
            0x0B6D, // purple-pansy stems
            0x0B6E, // purple-pansy buds
            0x0B71, // blue-pansy stems
            0x0B72, // blue-pansy buds
            0x0B75, // white-rose stems
            0x0B76, // white-rose buds
            0x0B79, // red-rose stems
            0x0B7A, // red-rose buds
            0x0B7D, // yellow-rose stems
            0x0B7E, // yellow-rose buds
            0x0B81, // pink-rose stems
            0x0B82, // pink-rose buds
            0x0B85, // orange-rose stems
            0x0B86, // orange-rose buds
            0x0B89, // purple-rose stems
            0x0B8A, // purple-rose buds
            0x0B8D, // black-rose stems
            0x0B8E, // black-rose buds
            0x0B91, // blue-rose stems
            0x0B92, // blue-rose buds
            0x0B95, // gold-rose stems
            0x0B96, // gold-rose buds
            0x0BA5, // white-lily stems
            0x0BA6, // white-lily buds
            0x0BA9, // red-lily stems
            0x0BAA, // red-lily buds
            0x0BAD, // yellow-lily stems
            0x0BAE, // yellow-lily buds
            0x0BB1, // pink-lily stems
            0x0BB2, // pink-lily buds
            0x0BB5, // orange-lily stems
            0x0BB6, // orange-lily buds
            0x0BB9, // black-lily stems
            0x0BBA, // black-lily buds
            0x0E7F, // white-windflower stems
            0x0E80, // white-windflower buds
            0x0E83, // orange-windflower stems
            0x0E84, // orange-windflower buds
            0x0E86, // blue-windflower stems
            0x0E87, // blue-windflower buds
            0x0E89, // pink-windflower stems
            0x0E8A, // pink-windflower buds
            0x0E8D, // red-windflower stems
            0x0E8E, // red-windflower buds
            0x0E90, // purple-windflower stems
            0x0E91, // purple-windflower buds
            0x0E94, // white-hyacinth stems
            0x0E95, // white-hyacinth buds
            0x0E98, // yellow-hyacinth stems
            0x0E98, // yellow-hyacinth buds
            0x0E9B, // pink-hyacinth stems
            0x0E9C, // pink-hyacinth buds
            0x0E9E, // orange-hyacinth stems
            0x0E9F, // orange-hyacinth buds
            0x0EA2, // red-hyacinth stems
            0x0EA3, // red-hyacinth buds
            0x0EA5, // blue-hyacinth stems
            0x0EA6, // blue-hyacinth buds
            0x0EA8, // purple-hyacinth stems
            0x0EA9, // purple-hyacinth buds
            0x0EAC, // white-mum stems
            0x0EAD, // white-mum buds
            0x0EB0, // yellow-mum stems
            0x0EB1, // yellow-mum buds
            0x0EB3, // purple-mum stems
            0x0EB4, // purple-mum buds
            0x0EB6, // pink-mum stems
            0x0EB7, // pink-mum buds
            0x0EBA, // red-mum stems
            0x0EBB, // red-mum buds
            0x0EC2, // green-mum stems
            0x0EC3, // green-mum buds
            0x0EEF, // red-cosmos sprouts
            0x0EF0, // red-cosmos plant
            0x0EF1, // white-cosmos sprouts
            0x0EF2, // white-cosmos plant
            0x0EF3, // yellow-cosmos sprouts
            0x0EF4, // yellow-cosmos plant
            0x0EF6, // pink-cosmos plant
            0x0EF8, // orange-cosmos plant
            0x0EFA, // black-cosmos plant
            0x0EFB, // white-tulip sprouts
            0x0EFC, // white-tulip plant
            0x0EFD, // red-tulip sprouts
            0x0EFE, // red-tulip plant
            0x0EFF, // yellow-tulip sprouts
            0x0F00, // yellow-tulip plant
            0x0F02, // pink-tulip plant
            0x0F04, // orange-tulip plant
            0x0F06, // purple-tulip plant
            0x0F08, // black-tulip plant
            0x0F09, // white-pansy sprouts
            0x0F0A, // white-pansy plant
            0x0F0B, // red-pansy sprouts
            0x0F0C, // red-pansy plant
            0x0F0D, // yellow-pansy sprouts
            0x0F0E, // yellow-pansy plant
            0x0F10, // orange-pansy plant
            0x0F12, // purple-pansy plant
            0x0F14, // blue-pansy plant
            0x0F15, // white-rose sprouts
            0x0F16, // white-rose plant
            0x0F17, // red-rose sprouts
            0x0F18, // red-rose plant
            0x0F19, // yellow-rose sprouts
            0x0F1A, // yellow-rose plant
            0x0F1C, // pink-rose plant
            0x0F1E, // orange-rose plant
            0x0F20, // purple-rose plant
            0x0F22, // black-rose plant
            0x0F24, // blue-rose plant
            0x0F26, // gold-rose plant
            0x0F2D, // white-lily sprouts
            0x0F2E, // white-lily plant
            0x0F2F, // red-lily sprouts
            0x0F30, // red-lily plant
            0x0F31, // yellow-lily sprouts
            0x0F32, // yellow-lily plant
            0x0F34, // pink-lily plant
            0x0F36, // orange-lily plant
            0x0F38, // black-lily plant
            0x0F39, // white-windflower sprouts
            0x0F3A, // white-windflower plant
            0x0F3B, // orange-windflower sprouts
            0x0F3C, // orange-windflower plant
            0x0F3E, // blue-windflower plant
            0x0F40, // pink-windflower plant
            0x0F41, // red-windflower sprouts
            0x0F42, // red-windflower plant
            0x0F44, // purple-windflower plant
            0x0F45, // white-hyacinth sprouts
            0x0F46, // white-hyacinth plant
            0x0F47, // yellow-hyacinth sprouts
            0x0F48, // yellow-hyacinth plant
            0x0F4A, // pink-hyacinth plant
            0x0F4C, // orange-hyacinth plant
            0x0F4D, // red-hyacinth sprouts
            0x0F4E, // red-hyacinth plant
            0x0F50, // blue-hyacinth plant
            0x0F52, // purple-hyacinth plant
            0x0F53, // white-mum sprouts
            0x0F54, // white-mum plant
            0x0F55, // yellow-mum sprouts
            0x0F56, // yellow-mum plant
            0x0F58, // purple-mum plant
            0x0F5A, // pink-mum plant
            0x0F5B, // red-mum sprouts
            0x0F5C, // red-mum plant
            0x0F5E, // green-mum plant
            0x1DE3, // lily-of-the-valley plant
        };
        private static readonly HashSet<UInt16> isRoseSet = new HashSet<UInt16>
        {
            0x0B75, // white-rose stems
            0x0B76, // white-rose buds
            0x0B79, // red-rose stems
            0x0B7A, // red-rose buds
            0x0B7D, // yellow-rose stems
            0x0B7E, // yellow-rose buds
            0x0B81, // pink-rose stems
            0x0B82, // pink-rose buds
            0x0B85, // orange-rose stems
            0x0B86, // orange-rose buds
            0x0B89, // purple-rose stems
            0x0B8A, // purple-rose buds
            0x0B8D, // black-rose stems
            0x0B8E, // black-rose buds
            0x0B91, // blue-rose stems
            0x0B92, // blue-rose buds
            0x0B95, // gold-rose stems
            0x0B96, // gold-rose buds
            0x0F15, // white-rose sprouts
            0x0F16, // white-rose plant
            0x0F17, // red-rose sprouts
            0x0F18, // red-rose plant
            0x0F19, // yellow-rose sprouts
            0x0F1A, // yellow-rose plant
            0x0F1C, // pink-rose plant
            0x0F1E, // orange-rose plant
            0x0F20, // purple-rose plant
            0x0F22, // black-rose plant
            0x0F24, // blue-rose plant
            0x0F26, // gold-rose plant
        };
        private static readonly HashSet<UInt16> isTreeSet = new HashSet<UInt16>
        {
            0x0AEC, // small young hardwood
            0x0AED, // medium young hardwood
            0x0AEE, // large young hardwood
            0x0AEF, // hardwood tree
            0x0AF2, // small young cedar
            0x0AF3, // medium young cedar
            0x0AF4, // large young cedar
            0x0AF5, // cedar tree
            0x0AF6, // nursery coconut
            0x0AF7, // small young coconut
            0x0AF8, // medium young coconut
            0x0AF9, // large young coconut
            0x0AFA, // coconut tree
            0x0AFB, // nursery bamboo
            0x0AFC, // small young bamboo
            0x0AFD, // medium young bamboo
            0x0AFE, // large young bamboo
            0x0AFF, // bamboo tree
            0x0D1C, // nursery apple
            0x0D1D, // small young apple
            0x0D1E, // medium young apple
            0x0D1F, // large young apple
            0x0D20, // apple tree
            0x0D21, // nursery orange
            0x0D22, // small young orange
            0x0D23, // medium young orange
            0x0D24, // large young orange
            0x0D25, // orange tree
            0x0D26, // nursery pear
            0x0D27, // small young pear
            0x0D28, // medium young pear
            0x0D29, // large young pear
            0x0D2A, // pear tree
            0x0D2B, // nursery peach
            0x0D2C, // small young peach
            0x0D2D, // medium young peach
            0x0D2E, // large young peach
            0x0D2F, // peach tree
            0x0D31, // nursery cherry
            0x0D32, // small young cherry
            0x0D33, // medium young cherry
            0x0D34, // large young cherry
            0x0D35, // cherry tree
            0x114A, // money tree
            0x1154, // nursery money tree
            0x1155, // small young money tree
            0x1156, // medium young money tree
            0x1157, // large young money tree
            0x141F, // nursery cedar
            0x1420, // nursery hardwood
        };
        private static readonly HashSet<UInt16> isFruitSet = new HashSet<UInt16>
        {
            0x08A5, // apple
            0x08A6, // orange
            0x08ED, // pear
            0x08EE, // peach
            0x08EF, // cherry
            0x08F0, // coconut
        };
        private static readonly HashSet<UInt16> isMushroomSet = new HashSet<UInt16>
        {
            0x0CCC, // elegant mushroom
            0x0CCD, // round mushroom
            0x0CCE, // skinny mushroom
            0x0CCF, // flat mushroom
            0x0CD0, // rare mushroom
        };
        private static readonly HashSet<UInt16> isCraftMaterialSet = new HashSet<UInt16>
        {
            0x09C4, // tree branch
            0x09C5, // bamboo piece
            0x0ACF, // softwood
            0x0AD0, // wood
            0x0AD1, // hardwood
            0x165F, // acorn
            0x1660, // pine cone
            0x1661, // young spring bamboo
            0x1662, // red ornament
            0x1663, // blue ornament
            0x1664, // gold ornament
            0x30DD, // earth egg
            0x30DE, // stone egg
            0x30DF, // leaf egg
            0x30E0, // wood egg
            0x30E1, // sky egg
            0x30E2, // water egg
            0x09C6, // stone
            0x09C9, // gold nugget
            0x09CF, // iron nugget
            0x0C12, // clay
            0x16E3, // cherry-blossom petal
            0x055E, // sea snail
            0x055F, // venus comb
            0x0560, // conch
            0x0563, // sand dollar
            0x0564, // coral
            0x0565, // giant clam
            0x0566, // cowrie
            0x175E, // summer shell
            0x0DD3, // snowflake
            0x0DD4, // large snowflake
            0x175F, // star fragment
            0x1760, // large star fragment
            0x1761, // Capricorn fragment
            0x1762, // Aquarius fragment
            0x1763, // Pisces fragment
            0x1764, // Aries fragment
            0x1765, // Taurus fragment
            0x1766, // Gemini fragment
            0x1767, // Cancer fragment
            0x1768, // Leo fragment
            0x1769, // Virgo fragment
            0x176A, // Libra fragment
            0x176B, // Scorpius fragment
            0x176C, // Sagittarius fragment
            0x0A40, // clump of weeds
        };
        private static readonly HashSet<UInt16> isDummyContainerSet = new HashSet<UInt16>
        {
            0x1225, // delivery
            0x1180, // present
            0x1095, // delivery box
            0x1E13, // present
            0x1E14, // present
            0x1E15, // present
            0x1E16, // present
            0x1E17, // present
            0x1E18, // present
            0x1E19, // present
            0x1E1A, // present
            0x1E1B, // present
            0x1E1C, // present
            0x1E1D, // present
            0x1E1E, // present
            0x1E1F, // present
            0x1E20, // present
            0x1E21, // present
            0x1E22, // present
        };
        private static readonly HashSet<UInt16> isFenceSet = new HashSet<UInt16>
        {
            0x0C08, // brick fence
            0x0D4A, // vertical-board fence
            0x0D4B, // bamboo lattice fence
            0x10FD, // corral fence
            0x10FE, // country fence
            0x1100, // rope fence
            0x1102, // imperial fence
            0x1104, // straw fence
            0x1105, // iron fence
            0x1106, // spiky fence
            0x1456, // iron-and-stone fence
            0x1457, // zen fence
            0x145C, // stone fence
            0x145D, // barbed-wire fence
            0x2DBF, // simple wooden fence
            0x2DC0, // lattice fence
            0x2FFD, // コミューン島専用柵
            0x3156, // Bunny Day fence
            0x31D6, // hedge
            0x325E, // wedding fence
        };
        private static readonly HashSet<UInt16> isFlowerSet = new HashSet<UInt16>
        {
            0x0900, // red cosmos
            0x0901, // white cosmos
            0x0B2F, // yellow cosmos
            0x0B33, // pink cosmos
            0x0B37, // orange cosmos
            0x0B3B, // black cosmos
            0x0B3F, // white tulips
            0x0B43, // red tulips
            0x0B47, // yellow tulips
            0x0B4B, // pink tulips
            0x0B4F, // orange tulips
            0x0B53, // purple tulips
            0x0B57, // black tulips
            0x0B5B, // white pansies
            0x0B5F, // red pansies
            0x0B63, // yellow pansies
            0x0B67, // orange pansies
            0x0B6B, // purple pansies
            0x0B6F, // blue pansies
            0x0B73, // white roses
            0x0B77, // red roses
            0x0B7B, // yellow roses
            0x0B7F, // pink roses
            0x0B83, // orange roses
            0x0B87, // purple roses
            0x0B8B, // black roses
            0x0B8F, // blue roses
            0x0B93, // gold roses
            0x0BA3, // white lilies
            0x0BA7, // red lilies
            0x0BAB, // yellow lilies
            0x0BAF, // pink lilies
            0x0BB3, // orange lilies
            0x0BB7, // black lilies
            0x0E7D, // white windflowers
            0x0E81, // orange windflowers
            0x0E85, // blue windflowers
            0x0E88, // pink windflowers
            0x0E8B, // red windflowers
            0x0E8F, // purple windflowers
            0x0E92, // white hyacinths
            0x0E96, // yellow hyacinths
            0x0E9A, // pink hyacinths
            0x0E9D, // orange hyacinths
            0x0EA0, // red hyacinths
            0x0EA4, // blue hyacinths
            0x0EA7, // purple hyacinths
            0x0EAA, // white mums
            0x0EAE, // yellow mums
            0x0EB2, // purple mums
            0x0EB5, // pink mums
            0x0EB8, // red mums
            0x1437, // green mums
        };
        private static readonly HashSet<UInt16> isFlowerSeedSet = new HashSet<UInt16>
        {
            0x0A42, // red-cosmos bag
            0x0A45, // white-cosmos bag
            0x0B30, // yellow-cosmos bag
            0x0B40, // white-tulip bag
            0x0B44, // red-tulip bag
            0x0B48, // yellow-tulip bag
            0x0B5C, // white-pansy bag
            0x0B60, // red-pansy bag
            0x0B64, // yellow-pansy bag
            0x0B74, // white-rose bag
            0x0B78, // red-rose bag
            0x0B7C, // yellow-rose bag
            0x0BA4, // white-lily bag
            0x0BA8, // red-lily bag
            0x0BAC, // yellow-lily bag
            0x0E7E, // white-windflower bag
            0x0E82, // orange-windflower bag
            0x0E8C, // red-windflower bag
            0x0E93, // white-hyacinth bag
            0x0E97, // yellow-hyacinth bag
            0x0EA1, // red-hyacinth bag
            0x0EAB, // white-mum bag
            0x0EAF, // yellow-mum bag
            0x0EB9, // red-mum bag
        };
        private static readonly HashSet<UInt16> isBushStartSet = new HashSet<UInt16>
        {
            0x0B04, // white-azalea start
            0x0BCB, // red-hibiscus start
            0x0BD0, // holly start
            0x0ECA, // pink-azalea start
            0x0ECE, // yellow-hibiscus start
            0x0ED2, // pink-hydrangea start
            0x0ED6, // blue-hydrangea start
            0x30BC, // yellow-tea-olive start
            0x30C0, // orange-tea-olive start
            0x30C7, // red-camellia start
            0x30CA, // pink-camellia start
        };
        private static readonly HashSet<UInt16> isWrappingPaperSet = new HashSet<UInt16>
        {
            0x1E03, // yellow wrapping paper
            0x1E04, // pink wrapping paper
            0x1E05, // orange wrapping paper
            0x1E06, // chartreuse wrapping paper
            0x1E07, // green wrapping paper
            0x1E08, // mint wrapping paper
            0x1E09, // light-blue wrapping paper
            0x1E0A, // purple wrapping paper
            0x1E0B, // navy wrapping paper
            0x1E0C, // blue wrapping paper
            0x1E0D, // white wrapping paper
            0x1E0E, // red wrapping paper
            0x1E0F, // gold wrapping paper
            0x1E10, // brown wrapping paper
            0x1E11, // gray wrapping paper
            0x1E12, // black wrapping paper
        };
        private static readonly HashSet<UInt16> hasQuantitySet = new HashSet<UInt16>(
            new UInt16[] {
                0x09D1, // Customization Kit
                0X11C5, // Fish Bait
                0X0CBF, // Wasp Nest
                0X094D, // communicator part
                0X0955, // rusted part
                0X0AD9, // medicine
                0X16DB, // Nook Miles Ticket
                0X26EC, // Bell voucher
                0X242D, // Saharah Ticket
                0X172C, // Tailor's Ticket
                0X0AC1, // party popper
                0X0A12, // pitfall seed
                0X0AEA, // Sapling
                0X0AF0, // Cedar Sapling
                0X08F7, // bamboo shoot
                0X125E, // Wisp spirit piece
                0X1CCE, // Maple leaves
                0X3102, // Heart crystals
                0X0ADB, // Birthday cupcake
                0X3331, // communicator
                0X32A8, // pearl
            }
            .Union(isCraftMaterialSet)
            .Union(isFenceSet)
            .Union(isFlowerSet)
            .Union(isFlowerSeedSet)
            .Union(isBushStartSet)
            .Union(isFruitSet)
            .Union(isMushroomSet)
            .Union(isWrappingPaperSet)
            );
        private static readonly HashSet<UInt16> containsItemSet = new HashSet<UInt16>(
            new UInt16[] {
                0x0A13, // Fossils contain the fossil
            }
            .Union(isTreeSet)
            .Union(isDummyContainerSet)
            );
        public static bool hasDurability(UInt16 item)
        {
            return hasDurabilitySet.Contains(item);
        }
        public static bool hasQuantity(UInt16 item)
        {
            return hasQuantitySet.Contains(item);
        }
        public static bool hasGenetics(UInt16 item)
        {
            return hasGeneticsSet.Contains(item);
        }
        public static bool isRose(UInt16 item)
        {
            return isRoseSet.Contains(item);
        }
        public static bool isTree(UInt16 item)
        {
            return isTreeSet.Contains(item);
        }
        public static bool containsItem(UInt16 item)
        {
            return containsItemSet.Contains(item);
        }
        public static bool isFlower(UInt16 item)
        {
            return isFlowerSet.Contains(item);
        }
    }
}
