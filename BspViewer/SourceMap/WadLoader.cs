using BspViewer.Extensions;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace BspViewer
{
    struct MissingTexture
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }

    public sealed class WadLoader
    {
        private BspMap _map;
        private string _mapFileName;
        private string[] _wadFileNames;
        
        private WadHeader _header;

        private BspTextureData[] _textureLookup;
        private List<MissingTexture> _missingTextures;

        public WadLoader(BspMap map, string mapFileName, string[] wadFileNames)
        {
            _map = map;

            _mapFileName = mapFileName;

            _wadFileNames = wadFileNames;
        }

        public BspTextureData[] Load()
        {
            using (var mapReader = new BinaryReader(new FileStream(_mapFileName, FileMode.Open)))
            {
                foreach (var fileName in _wadFileNames)
                {
                    if (!File.Exists(fileName))
                    {
                        throw new FileNotFoundException(fileName);
                    }

                    using (var reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
                    {
                        _header = ReadHeader(reader);
                    }

                    LoadTextures(mapReader);
                }
            }

            return _textureLookup;
        }

        private WadHeader ReadHeader(BinaryReader reader)
        {
            var header = new WadHeader
            {
                Magic = reader.ReadString(4),
                Dirs = reader.ReadInt32(),
                DirOffset = reader.ReadInt32()
            };

            if (header.Magic != "WAD2" && header.Magic != "WAD3")
            {
                throw new InvalidDataException();
            }

            return header;
        }

        #region BSP Textures

        private void LoadTextures(BinaryReader mapReader)
        {
            // Texture Coordinates

            var textureCoordinates = new List<FaceCoord>();

            for (var i = 0; i < _map.Faces.Length; i++)
            {
                var face = _map.Faces[i];
                var texInfo = _map.TextureInfos[face.TextureInfo];

                var faceCoords = new List<FaceCoord>();

                for (var j = 0; j < face.NumEdges; j++)
                {
                    var edgeIndex = _map.SurfEdges[face.FirstEdge + j];

                    ushort vertexIndex;
                    if (edgeIndex > 0)
                    {
                        var edge = _map.Edges[edgeIndex];
                        vertexIndex = edge.Vertices[0];
                    }
                    else
                    {
                        edgeIndex *= -1;
                        var edge = _map.Edges[edgeIndex];
                        vertexIndex = edge.Vertices[1];
                    }

                    var vertex = _map.Vertices[vertexIndex];
                    var mipTexture = _map.MipTextures[texInfo.MipTex];

                    var coord = new FaceCoord
                    {
                        Vec3s = (MathLib.DotProduct(vertex.Point, texInfo.Vec3s) + texInfo.SShift) / mipTexture.Width,
                        Vec3t = (MathLib.DotProduct(vertex.Point, texInfo.Vec3t) + texInfo.TShift) / mipTexture.Height
                    };

                    faceCoords.Add(coord);
                }

                textureCoordinates.AddRange(faceCoords);
            }

            var whiteTexture = new BspTextureData(); // ToDo

            // Texture images

            _textureLookup = new BspTextureData[_map.Faces.Length];
            _missingTextures = new List<MissingTexture>();

            for (var i = 0; i < _map.MipTextures.Length; i++)
            {
                var mipTexture = _map.MipTextures[i];

                if (mipTexture.Offsets[0] == 0)
                {
                    // External texture

                    // search texture in loaded wads
                    var texture = LoadTextureFromWad(mipTexture.Name);

                    if (texture.TextureData != null)
                    {
                        // the texture has been found in a loaded wad
                        _textureLookup[i] = texture;

                        Debug.WriteLine("Texture " + mipTexture.Name + " found");
                    }
                    else
                    {
                        // bind simple white texture to do not disturb lightmaps
                        _textureLookup[i] = whiteTexture;

                        // store the name and position of this missing texture,
                        // so that it can later be loaded to the right position by calling loadMissingTextures()
                        _missingTextures.Add(new MissingTexture { Name = mipTexture.Name, Index = i });

                        Debug.WriteLine("Texture " + mipTexture.Name + " is missing");
                    }

                    continue;
                }
                else
                {
                    // Load internal texture if present

                    // Calculate offset of the texture in the bsp file
                    //var offset = _map.TextureHeader.Offsets[i];
                    var offset = _map.Header.Lumps[BspDefs.LUMP_TEXTURES].Offset + _map.TextureHeader.Offsets[i];

                    _textureLookup[i] = FetchTextureAtOffset(mapReader, offset);

                    Debug.WriteLine("Fetched interal texture " + mipTexture.Name);
                }
            }
        }

        private BspTextureData FetchTextureAtOffset(BinaryReader mapReader, long offset)
        {
            // Seek to the texture beginning
            mapReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            // Load texture header
            var mipTex = new BspMipTexture()
            {
                Name = mapReader.ReadString(WadDefs.MAXTEXTURENAME),
                Width = mapReader.ReadUInt32(),
                Height = mapReader.ReadUInt32(),
                Offsets = mapReader.ReadUInt32Array(WadDefs.MIPLEVELS)
            };

            // Fetch color palette
            var paletteOffset = mipTex.Offsets[WadDefs.MIPLEVELS - 1] + ((mipTex.Width / 8) * (mipTex.Height / 8)) + 2;
            var palette = mapReader.ReadUInt8Array(offset + paletteOffset, 256 * 3);

            // Width and height shrink to half for every level
            int width = (int) mipTex.Width;
            int height = (int) mipTex.Height;

            // Fetch the indexed texture
            var textureIndexes = mapReader.ReadUInt8Array(offset + mipTex.Offsets[0], width * height);

            // Allocate storage for the rgba texture
            var textureData = new float[width * height * 4];

            // Translate the texture from indexes to rgba
            for (var j = 0; j < width * height; j++)
            {
                var paletteIndex = textureIndexes[j] * 3;

                textureData[j * 4] = palette[paletteIndex];
                textureData[j * 4 + 1] = palette[paletteIndex + 1];
                textureData[j * 4 + 2] = palette[paletteIndex + 2];
                textureData[j * 4 + 3] = 255; // every pixel is totally opaque
            }

            if (mipTex.Name.Substring(0, 1) == "{") // this is an alpha texture
            {
                Debug.WriteLine(mipTex.Name + " is an alpha texture");

                // Transfere alpha key color to actual alpha values
                ApplyAlphaSections(ref textureData, width, height, palette[255 * 3 + 0], palette[255 * 3 + 1], palette[255 * 3 + 2]);
            }

            return new BspTextureData(textureData, width, height, mipTex.Name);
        }

        private BspTextureData LoadTextureFromWad(string name)
        {
            return new BspTextureData(); // ToDo
        }

        private void ApplyAlphaSections(ref float[] pixels, int width, int height, uint keyR, uint keyG, uint keyB)
        {
            // Create an equally sized pixel buffer initialized to the key color 
            var rgbBuffer = new float[width * height * 3];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var bufIndex = (y * width + x) * 3;
                    rgbBuffer[bufIndex + 0] = keyR;
                    rgbBuffer[bufIndex + 1] = keyG;
                    rgbBuffer[bufIndex + 2] = keyB;
                }
            }

            // The key color signifies a transparent portion of the texture. Zero alpha for blending and
            // to get rid of key colored edges choose the average color of the nearest non key pixels.

            // Interpolate colors for transparent pixels
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var index = (y * width + x) * 4;

                    if ((pixels[index + 0] == keyR) &&
                        (pixels[index + 1] == keyG) &&
                        (pixels[index + 2] == keyB))
                    {
                        // This is a pixel which should be transparent

                        pixels[index + 3] = 0;

                        var count = 0;
                        var colorSum = new float[3];
                        colorSum[0] = 0;
                        colorSum[1] = 0;
                        colorSum[2] = 0;

                        // left above pixel
                        if ((x > 0) && (y > 0))
                        {
                            var pixelIndex = ((y - 1) * width + (x - 1)) * 4;
                            if (pixels[pixelIndex + 0] != keyR ||
                                pixels[pixelIndex + 1] != keyG ||
                                pixels[pixelIndex + 2] != keyB)
                            {
                                colorSum[0] += pixels[pixelIndex + 0] * MathLib.SQRT2;
                                colorSum[1] += pixels[pixelIndex + 1] * MathLib.SQRT2;
                                colorSum[2] += pixels[pixelIndex + 2] * MathLib.SQRT2;
                                count++;
                            }
                        }

                        // above pixel
                        if ((x >= 0) && (y > 0))
                        {
                            var pixelIndex = ((y - 1) * width + x) * 4;
                            if (pixels[pixelIndex + 0] != keyR ||
                                pixels[pixelIndex + 1] != keyG ||
                                pixels[pixelIndex + 2] != keyB)
                            {
                                colorSum[0] += pixels[pixelIndex + 0];
                                colorSum[1] += pixels[pixelIndex + 1];
                                colorSum[2] += pixels[pixelIndex + 2];
                                count++;
                            }
                        }

                        // right above pixel
                        if ((x < width - 1) && (y > 0))
                        {
                            var pixelIndex = ((y - 1) * width + (x + 1)) * 4;
                            if (pixels[pixelIndex + 0] != keyR ||
                                pixels[pixelIndex + 1] != keyG ||
                                pixels[pixelIndex + 2] != keyB)
                            {
                                colorSum[0] += pixels[pixelIndex + 0] * MathLib.SQRT2;
                                colorSum[1] += pixels[pixelIndex + 1] * MathLib.SQRT2;
                                colorSum[2] += pixels[pixelIndex + 2] * MathLib.SQRT2;
                                count++;
                            }
                        }

                        // left pixel
                        if (x > 0)
                        {
                            var pixelIndex = (y * width + (x - 1)) * 4;
                            if (pixels[pixelIndex + 0] != keyR ||
                                pixels[pixelIndex + 1] != keyG ||
                                pixels[pixelIndex + 2] != keyB)
                            {
                                colorSum[0] += pixels[pixelIndex + 0];
                                colorSum[1] += pixels[pixelIndex + 1];
                                colorSum[2] += pixels[pixelIndex + 2];
                                count++;
                            }
                        }

                        // right pixel
                        if (x < width - 1)
                        {
                            var pixelIndex = (y * width + (x + 1)) * 4;
                            if (pixels[pixelIndex + 0] != keyR ||
                                pixels[pixelIndex + 1] != keyG ||
                                pixels[pixelIndex + 2] != keyB)
                            {
                                colorSum[0] += pixels[pixelIndex + 0];
                                colorSum[1] += pixels[pixelIndex + 1];
                                colorSum[2] += pixels[pixelIndex + 2];
                                count++;
                            }
                        }

                        // left underneath pixel
                        if ((x > 0) && (y < height - 1))
                        {
                            var pixelIndex = ((y + 1) * width + (x - 1)) * 4;
                            if (pixels[pixelIndex + 0] != keyR ||
                                pixels[pixelIndex + 1] != keyG ||
                                pixels[pixelIndex + 2] != keyB)
                            {
                                colorSum[0] += pixels[pixelIndex + 0] * MathLib.SQRT2;
                                colorSum[1] += pixels[pixelIndex + 1] * MathLib.SQRT2;
                                colorSum[2] += pixels[pixelIndex + 2] * MathLib.SQRT2;
                                count++;
                            }
                        }

                        // underneath pixel
                        if ((x >= 0) && (y < height - 1))
                        {
                            var pixelIndex = ((y + 1) * width + x) * 4;
                            if (pixels[pixelIndex + 0] != keyR ||
                                pixels[pixelIndex + 1] != keyG ||
                                pixels[pixelIndex + 2] != keyB)
                            {
                                colorSum[0] += pixels[pixelIndex + 0];
                                colorSum[1] += pixels[pixelIndex + 1];
                                colorSum[2] += pixels[pixelIndex + 2];
                                count++;
                            }
                        }

                        // right underneath pixel
                        if ((x < width - 1) && (y < height - 1))
                        {
                            var pixelIndex = ((y + 1) * width + (x + 1)) * 4;
                            if (pixels[pixelIndex + 0] != keyR ||
                                pixels[pixelIndex + 1] != keyG ||
                                pixels[pixelIndex + 2] != keyB)
                            {
                                colorSum[0] += pixels[pixelIndex + 0] * MathLib.SQRT2;
                                colorSum[1] += pixels[pixelIndex + 1] * MathLib.SQRT2;
                                colorSum[2] += pixels[pixelIndex + 2] * MathLib.SQRT2;
                                count++;
                            }
                        }

                        if (count > 0)
                        {
                            colorSum[0] /= count;
                            colorSum[1] /= count;
                            colorSum[2] /= count;

                            var bufIndex = (y * width + x) * 3;
                            rgbBuffer[bufIndex + 0] = (float) Math.Round(colorSum[0]);
                            rgbBuffer[bufIndex + 1] = (float) Math.Round(colorSum[1]);
                            rgbBuffer[bufIndex + 2] = (float) Math.Round(colorSum[2]);
                        }
                    }
                }
            }

            // Transfer interpolated colors to the texture
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var index = (y * width + x) * 4;
                    var bufindex = (y * width + x) * 3;

                    if ((rgbBuffer[bufindex + 0] != keyR) ||
                        (rgbBuffer[bufindex + 1] != keyG) ||
                        (rgbBuffer[bufindex + 2] != keyB))
                    {
                        pixels[index + 0] = rgbBuffer[bufindex + 0];
                        pixels[index + 1] = rgbBuffer[bufindex + 1];
                        pixels[index + 2] = rgbBuffer[bufindex + 2];
                    }
                }
            }
        }

        #endregion
    }
}