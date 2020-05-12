using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackRat {
    public class IconManager {
        //private Dictionary<string, Bitmap> SmallImages;
        //private Dictionary<string, Bitmap> MediumImages;
        private Dictionary<string, Bitmap> Images;
        //public readonly Dictionary<string, Bitmap> Images;

        public enum Size : byte {
            SMALL = 1, // 16
            MEDIUM = 2, // 32
            LARGE = 3 // 64
            }

        //CURRENT get icons! D:<

        /// <summary>
        /// Returns a bitmap-icon representation of the [extension] given
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <param name="size">Size Flag (SMALL=16 MEDIUM=32 LARGE=64)</param>
        /// <returns>Bitmap Icon</returns>
        public Bitmap this[string extension, Size size = Size.SMALL] {
            get {
                if (Images.ContainsKey(extension.ToLower())) {
                    return Images[size + "|" + extension.ToLower()];
                    }
                else {
                    //TODO Fetch Icon for 'extension' at size Enum.Size and store
                    return null;
                    }
                }
            set {
                Images[extension.ToLower()] = value;
                }
            }

        public IconManager() {
            Images = new Dictionary<string, Bitmap>();
            }
        }
    }
