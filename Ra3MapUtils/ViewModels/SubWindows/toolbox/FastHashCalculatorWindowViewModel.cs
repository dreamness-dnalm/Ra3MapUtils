using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfClipboard = System.Windows.Clipboard;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class FastHashCalculatorWindowViewModel : ObservableObject
{
    [ObservableProperty] private bool _isTopmost = false;

    [ObservableProperty] private bool _isCaseInsensitive = true;

    [ObservableProperty] private bool _isHexOutput = false;

    [ObservableProperty] private string _inputText = "";

    [ObservableProperty] private string _outputText = "";

    partial void OnInputTextChanged(string value)
    {
        RefreshOutput();
    }

    partial void OnIsCaseInsensitiveChanged(bool value)
    {
        RefreshOutput();
    }

    partial void OnIsHexOutputChanged(bool value)
    {
        RefreshOutput();
    }

    [RelayCommand]
    private void CopyOutput()
    {
        if (string.IsNullOrEmpty(OutputText))
        {
            return;
        }

        try
        {
            WpfClipboard.SetDataObject(OutputText, true);
        }
        catch
        {
            // 剪贴板被其他程序占用时忽略，避免影响工具窗口。
        }
    }

    private void RefreshOutput()
    {
        if (string.IsNullOrEmpty(InputText))
        {
            OutputText = "";
            return;
        }

        var lines = SplitLines(InputText);
        var builder = new StringBuilder();
        for (var index = 0; index < lines.Length; index++)
        {
            if (index > 0)
            {
                builder.AppendLine();
            }

            builder.Append(FormatHash(ComputeFastHash(lines[index], IsCaseInsensitive), IsHexOutput));
        }

        OutputText = builder.ToString();
    }

    public static uint ComputeFastHash(string text, bool caseInsensitive)
    {
        var normalizedText = caseInsensitive ? text.ToLowerInvariant() : text;
        return ComputeFastHash(Encoding.UTF8.GetBytes(normalizedText));
    }

    private static uint ComputeFastHash(ReadOnlySpan<byte> buffer)
    {
        unchecked
        {
            var length = buffer.Length;
            var hash = (uint)length;
            if (length == 0)
            {
                return 0u;
            }

            var remainder = length & 3;
            var offset = 0;
            for (var blockCount = length >> 2; blockCount != 0; blockCount--)
            {
                hash += BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(offset, 2));
                hash ^= ((uint)BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(offset + 2, 2)) ^ (hash << 5)) << 11;
                hash += hash >> 11;
                offset += 4;
            }

            switch (remainder)
            {
                case 1:
                    hash += buffer[offset];
                    hash ^= hash << 10;
                    hash += hash >> 1;
                    break;
                case 2:
                    hash += BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(offset, 2));
                    hash ^= hash << 11;
                    hash += hash >> 17;
                    break;
                case 3:
                    hash += BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(offset, 2));
                    hash ^= hash << 16;
                    hash ^= (uint)buffer[offset + 2] << 18;
                    hash += hash >> 11;
                    break;
            }

            hash ^= hash << 3;
            hash += hash >> 5;
            hash ^= hash << 2;
            hash += hash >> 15;
            hash ^= hash << 10;
            return hash;
        }
    }

    private static string[] SplitLines(string text)
    {
        return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    }

    private static string FormatHash(uint hash, bool isHexOutput)
    {
        return isHexOutput
            ? "0x" + hash.ToString("X", CultureInfo.InvariantCulture)
            : hash.ToString(CultureInfo.InvariantCulture);
    }
}
