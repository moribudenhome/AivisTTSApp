using System.Text;
using System.Text.Json;
using NAudio.CoreAudioApi;
using NAudio.Wave;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class StyleItem
{
    public int Id { get; }
    public string Label { get; }
    public StyleItem(int id, string label) { Id = id; Label = label; }
    public override string ToString() => Label;
}

public class MainForm : Form
{
    // ---------- UI Elements ----------
    private readonly ComboBox _cbDevices = new() { Dock = DockStyle.Fill, DropDownWidth = 500 };
    private readonly TextBox  _txtServer = new() { Text = "http://localhost:10101", Dock = DockStyle.Fill };
    private readonly ComboBox _cbModel   = new() { Dock = DockStyle.Fill, DropDownWidth = 500 };
    private readonly Button   _btnRefreshModels = new() { Text = "モデル更新" };
    private readonly Button   _btnToggle  = new() { Text = "▼ 詳細", Width = 70 };
    private readonly TextBox  _txtInput   = new() { Multiline = true, Height = 100, Dock = DockStyle.Fill };
    private readonly Button   _btnSpeak   = new() { Text = "Speak", Dock = DockStyle.Right };
    private readonly CheckBox _chkTopMost = new() { Text = "Always on Top" };
    private readonly Panel    _advancedPanel = new() { AutoSize = true, Dock = DockStyle.Top };

    private readonly HttpClient _http = new();

    public MainForm()
    {
        Text = "AIVIS TTS Client";
        MinimumSize = new System.Drawing.Size(300, 180);
        Size = new System.Drawing.Size(650, 330);

        BuildLayout();
        BindEvents();
    }

    // ---------- Layout ----------
    private void BuildLayout()
    {
        var adv = new TableLayoutPanel { ColumnCount = 3, AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(0, 5, 0, 5) };
        adv.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        adv.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        adv.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        adv.Controls.Add(new Label { Text = "Playback Device" }, 0, 0);
        adv.Controls.Add(_cbDevices, 1, 0);

        adv.Controls.Add(new Label { Text = "AIVIS Server" }, 0, 1);
        adv.Controls.Add(_txtServer, 1, 1);

        adv.Controls.Add(new Label { Text = "Voice Model" }, 0, 2);
        adv.Controls.Add(_cbModel, 1, 2);
        adv.Controls.Add(_btnRefreshModels, 2, 2);

        _advancedPanel.Controls.Add(adv);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4 };
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        main.Controls.Add(_btnToggle, 0, 0);
        main.Controls.Add(_advancedPanel, 0, 1);
        main.Controls.Add(_txtInput, 0, 2);

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        bottom.Controls.Add(_chkTopMost);
        bottom.Controls.Add(_btnSpeak);
        main.Controls.Add(bottom, 0, 3);

        Controls.Add(main);
    }

    private void BindEvents()
    {
        Load += async (_, __) => await InitializeAsync();
        _chkTopMost.CheckedChanged += (_, __) => TopMost = _chkTopMost.Checked;
        _btnSpeak.Click += async (_, __) => await SpeakAsync();
        _btnRefreshModels.Click += async (_, __) => await LoadModelsAsync(true);
        _btnToggle.Click += (_, __) => ToggleAdvanced();
        _txtInput.KeyDown += async (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SpeakAsync();
            }
        };
    }

    private void ToggleAdvanced()
    {
        _advancedPanel.Visible = !_advancedPanel.Visible;
        _btnToggle.Text = _advancedPanel.Visible ? "▲ 詳細" : "▼ 詳細";
        MinimumSize = _advancedPanel.Visible ? new System.Drawing.Size(300, 180) : new System.Drawing.Size(300, 120);
    }

    // ---------- Initialization ----------
    private async Task InitializeAsync()
    {
        foreach (var dev in new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            _cbDevices.Items.Add(dev.FriendlyName);
        if (_cbDevices.Items.Count > 0) _cbDevices.SelectedIndex = 0;

        await LoadModelsAsync(showError: true);
    }

    // ---------- Model Loading ----------
    private async Task LoadModelsAsync(bool showError)
    {
        var baseUrl = _txtServer.Text.TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl)) { if (showError) MessageBox.Show("AIVIS サーバー URL を入力してください。", "Info"); return; }

        _btnRefreshModels.Enabled = false; Cursor = Cursors.WaitCursor;
        try
        {
            var resp = await _http.GetAsync($"{baseUrl}/speakers");
            resp.EnsureSuccessStatusCode();
            var speakers = await JsonSerializer.DeserializeAsync<List<SpeakerDto>>(await resp.Content.ReadAsStreamAsync()) ?? new List<SpeakerDto>();

            _cbModel.Items.Clear();
            foreach (var sp in speakers)
                foreach (var st in sp.styles)
                    _cbModel.Items.Add(new StyleItem(st.id, $"{sp.name} - {st.name} (ID:{st.id})"));

            if (_cbModel.Items.Count > 0) _cbModel.SelectedIndex = 0;
            else if (showError) MessageBox.Show("スタイルが見つかりません。", "Warning");
        }
        catch (Exception ex) when (showError)
        {
            MessageBox.Show($"AivisSpeech Engine に接続できません。\nURL: {baseUrl}\n{ex.Message}", "Connection Error");
        }
        finally
        {
            _btnRefreshModels.Enabled = true; Cursor = Cursors.Default;
        }
    }

    // ---------- Speak ----------
    private async Task SpeakAsync()
    {
        var text = _txtInput.Text.Trim(); if (string.IsNullOrEmpty(text)) return;
        if (_cbDevices.SelectedIndex < 0) return;
        if (_cbModel.SelectedItem is not StyleItem item) { MessageBox.Show("モデルが選択されていません。", "Info"); return; }
        var baseUrl = _txtServer.Text.TrimEnd('/'); if (string.IsNullOrEmpty(baseUrl)) { MessageBox.Show("AIVIS サーバー URL を入力してください。", "Error"); return; }

        _btnSpeak.Enabled = false; Cursor = Cursors.WaitCursor;
        try
        {
            var wav = await GenerateSpeechAsync(baseUrl, text, item.Id);
            await PlayWaveAsync(wav, _cbDevices.SelectedIndex);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"発話失敗: {ex.Message}", "Error");
        }
        finally
        {
            _btnSpeak.Enabled = true; Cursor = Cursors.Default;
        }
    }

    private async Task<byte[]> GenerateSpeechAsync(string baseUrl, string text, int speakerId)
    {
        var queryUrl = $"{baseUrl}/audio_query?text={Uri.EscapeDataString(text)}&speaker={speakerId}";
        var qResp = await _http.PostAsync(queryUrl, null); qResp.EnsureSuccessStatusCode();
        var qJson = await qResp.Content.ReadAsStringAsync();

        var synthUrl = $"{baseUrl}/synthesis?speaker={speakerId}";
        var sResp = await _http.PostAsync(synthUrl, new StringContent(qJson, Encoding.UTF8, "application/json")); sResp.EnsureSuccessStatusCode();
        return await sResp.Content.ReadAsByteArrayAsync();
    }

    private static async Task PlayWaveAsync(byte[] wav, int deviceIndex)
    {
        using var reader = new WaveFileReader(new MemoryStream(wav));
        var dev = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)[deviceIndex];
        using var resampler = new MediaFoundationResampler(reader, dev.AudioClient.MixFormat) { ResamplerQuality = 60 };
        using var output = new WasapiOut(dev, AudioClientShareMode.Shared, false, 50);
        output.Init(resampler);
        output.Play();
        while (output.PlaybackState == PlaybackState.Playing) await Task.Delay(50);
    }

    // ---------- DTO ----------
    internal record SpeakerDto(string name, List<StyleDto> styles);
    internal record StyleDto(int id, string name);
}
