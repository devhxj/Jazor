#!/usr/bin/env dotnet run

using System.Buffers.Binary;

var repositoryRoot = FindRepositoryRoot();
var outputPath = Path.Combine(repositoryRoot, "samples", "JazorAdmin", "wwwroot", "favicon.ico");
var checkOnly = args.Any(static argument => string.Equals(argument, "--check", StringComparison.Ordinal));
var icon = BuildIcon([16, 32, 48, 64]);

if (checkOnly)
{
    if (!File.Exists(outputPath) || !File.ReadAllBytes(outputPath).AsSpan().SequenceEqual(icon))
        throw new InvalidDataException("JazorAdmin favicon.ico is stale. Run generate-jazoradmin-brand-assets.cs.");

    Console.WriteLine("JazorAdmin brand assets are current.");
    return;
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllBytes(outputPath, icon);
Console.WriteLine("Generated " + Path.GetRelativePath(repositoryRoot, outputPath));

static string FindRepositoryRoot()
{
    for (var directory = new DirectoryInfo(Environment.CurrentDirectory); directory is not null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    }

    throw new DirectoryNotFoundException("Could not find the Jazor repository root.");
}

static byte[] BuildIcon(int[] sizes)
{
    var images = sizes.Select(BuildImage).ToArray();
    var directoryLength = 6 + (images.Length * 16);
    var icon = new byte[directoryLength + images.Sum(static image => image.Length)];
    var span = icon.AsSpan();
    BinaryPrimitives.WriteUInt16LittleEndian(span, 0);
    BinaryPrimitives.WriteUInt16LittleEndian(span[2..], 1);
    BinaryPrimitives.WriteUInt16LittleEndian(span[4..], checked((ushort)images.Length));

    var imageOffset = directoryLength;
    for (var index = 0; index < sizes.Length; index++)
    {
        var entry = span.Slice(6 + (index * 16), 16);
        entry[0] = checked((byte)sizes[index]);
        entry[1] = checked((byte)sizes[index]);
        BinaryPrimitives.WriteUInt16LittleEndian(entry[4..], 1);
        BinaryPrimitives.WriteUInt16LittleEndian(entry[6..], 32);
        BinaryPrimitives.WriteInt32LittleEndian(entry[8..], images[index].Length);
        BinaryPrimitives.WriteInt32LittleEndian(entry[12..], imageOffset);
        images[index].CopyTo(span[imageOffset..]);
        imageOffset += images[index].Length;
    }

    return icon;
}

static byte[] BuildImage(int size)
{
    var pixelBytes = size * size * 4;
    var maskStride = ((size + 31) / 32) * 4;
    var image = new byte[40 + pixelBytes + (maskStride * size)];
    var header = image.AsSpan();
    BinaryPrimitives.WriteInt32LittleEndian(header, 40);
    BinaryPrimitives.WriteInt32LittleEndian(header[4..], size);
    BinaryPrimitives.WriteInt32LittleEndian(header[8..], size * 2);
    BinaryPrimitives.WriteUInt16LittleEndian(header[12..], 1);
    BinaryPrimitives.WriteUInt16LittleEndian(header[14..], 32);
    BinaryPrimitives.WriteInt32LittleEndian(header[20..], pixelBytes);

    for (var y = 0; y < size; y++)
    {
        for (var x = 0; x < size; x++)
        {
            var color = SamplePixel(x, y, size);
            // DIB icon pixels are BGRA and stored bottom-up. The empty AND mask preserves alpha semantics.
            var offset = 40 + (((size - y - 1) * size + x) * 4);
            image[offset] = color.Blue;
            image[offset + 1] = color.Green;
            image[offset + 2] = color.Red;
            image[offset + 3] = color.Alpha;
        }
    }

    return image;
}

static Pixel SamplePixel(int pixelX, int pixelY, int size)
{
    const int samplesPerAxis = 4;
    var red = 0;
    var green = 0;
    var blue = 0;
    var alpha = 0;
    for (var sampleY = 0; sampleY < samplesPerAxis; sampleY++)
    {
        for (var sampleX = 0; sampleX < samplesPerAxis; sampleX++)
        {
            var x = (((pixelX + ((sampleX + 0.5) / samplesPerAxis)) / size) * 2) - 1;
            var y = (((pixelY + ((sampleY + 0.5) / samplesPerAxis)) / size) * 2) - 1;
            var color = Paint(x, y);
            red += color.Red;
            green += color.Green;
            blue += color.Blue;
            alpha += color.Alpha;
        }
    }

    var divisor = samplesPerAxis * samplesPerAxis;
    return new Pixel((byte)(red / divisor), (byte)(green / divisor), (byte)(blue / divisor), (byte)(alpha / divisor));
}

static Pixel Paint(double x, double y)
{
    var radius = Math.Sqrt((x * x) + (y * y));
    if (radius > 0.94)
        return default;

    var color = new Pixel(16, 60, 61, 255);
    if (radius > 0.84)
        color = new Pixel(227, 195, 99, 255);
    else if (radius > 0.80)
        color = new Pixel(27, 89, 84, 255);

    for (var turn = 0; turn < 4; turn++)
    {
        var angle = turn * Math.PI / 2;
        var localX = (Math.Cos(angle) * x) + (Math.Sin(angle) * y);
        var localY = (-Math.Sin(angle) * x) + (Math.Cos(angle) * y);
        if (ContainsTopBird(localX, localY))
            color = new Pixel(240, 203, 111, 255);
    }

    if (radius < 0.2625)
        color = new Pixel(232, 201, 110, 255);
    if (radius < 0.1094)
        color = new Pixel(22, 80, 77, 255);
    if (radius < 0.0406)
        color = new Pixel(249, 230, 168, 255);

    return color;
}

static bool ContainsTopBird(double x, double y)
{
    ReadOnlySpan<Point> points =
    [
        new(-0.56, -0.10), new(-0.38, -0.47), new(-0.13, -0.63), new(0.22, -0.69),
        new(0.09, -0.56), new(-0.03, -0.41), new(0.19, -0.44), new(0.47, -0.31),
        new(0.22, -0.25), new(0.0, -0.16), new(-0.13, -0.03), new(-0.28, -0.13)
    ];
    var contains = false;
    for (var index = 0; index < points.Length; index++)
    {
        var current = points[index];
        var previous = points[(index + points.Length - 1) % points.Length];
        if ((current.Y > y) == (previous.Y > y))
            continue;

        var crossingX = ((previous.X - current.X) * (y - current.Y) / (previous.Y - current.Y)) + current.X;
        if (x < crossingX)
            contains = !contains;
    }

    return contains;
}

readonly record struct Point(double X, double Y);
readonly record struct Pixel(byte Red, byte Green, byte Blue, byte Alpha);
