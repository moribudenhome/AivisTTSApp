# Aivis TTS Client

FiveM などのゲームで **マイクが使えない／使いたくない** 場面でも、音声合成 (TTS) でVCが使える Windows専用 アプリケーションです。

---

## 概要

- **ホットキー** … `Ctrl + Enter` で即発話。
- **入力 UI** … ゲーム画面上に常時最前面で小窓表示。
- **TTS エンジン** … ローカルの *AivisSpeech Engine* (`http://localhost:10101`) を利用。
- **出力デバイス** … 任意の再生デバイスを選択可能（ゲームに流したい場合は VB‑CABLE など仮想デバイスを利用）。
- **詳細パネル** … 再生デバイス／サーバー URL／ボイスモデルを切替可。`▼ 詳細` で折りたたみ。

---

## 動作環境

| 項目   | 推奨                        |
| ---- | ------------------------- |
| OS   | Windows 10 / 11           |
| .NET | .NET 6 以上      |
| TTS  | AivisSpeech Engine 1.1 以上 |

---

## インストール手順

### AivisSpeech

1. 公式サイト [https://aivis-project.com/](https://aivis-project.com/) にアクセス。
2. トップページの **「AivisSpeechをダウンロード」** からダウンロードし、インストーラを起動。
3. インストール完了後、インストール先に作成される `AivisSpeech-Engine` フォルダ内の `run.exe` をダブルクリックして `AivisSpeach-Engine` を起動
   - 例: `C:\Users\<ユーザー名>\AppData\Local\Programs\AivisSpeech\AivisSpeech-Engine\run.exe`

### Aivis TTS Client (本アプリ)

リリースページの ZIP を展開し、`AivisTTSClient.exe` を実行するだけで利用できます。

---

## 基本的な使い方

1. **AivisSpeech Engine** を先に起動 (デフォルト: `localhost:10101`)。
2. `AIVIS TTS Client` を起動。
3. モデル一覧が自動取得され、`Voice Model` プルダウンに話者 + スタイルが表示されます。
4. テキスト欄に発話内容を入力し `Speak` ボタン、または **`Ctrl + Enter`** で即時読み上げ。
5. `Always on Top` でゲーム画面の上に固定。
6. `▼ 詳細` をクリックすると各種設定を隠して **コンパクトモード**。

---

## FiveM で利用する場合

FiveM のボイスチャットへ TTS を流すには **VB‑CABLE** を仮想マイクとして利用する方法が簡単です。

### VB‑CABLE のセットアップ

1. [https://vb-audio.com/Cable/](https://vb-audio.com/Cable/) から **VB‑CABLE Driver** をダウンロード。
2. `VBCABLE_Setup_x64.exe` を **管理者権限で実行** → *Install Driver*。
3. **インストール後に Windows を再起動**。
4. 再起動後、`サウンド設定 > 再生 / 録音` に `VB‑Audio Cable Input / Output` が追加されていれば成功。

### FiveM への音声ルーティング

| 手順 | 設定内容                                                                         |
| -- | ---------------------------------------------------------------------------- |
| 1  | **Aivis TTS Client の再生デバイス** → CABLE Input (VB-Audio Virtual Cable) を選択      |
| 2  | **FiveM > Settings > Voice Chat** → *Input Device* = `VB‑Audio Cable Output` |
| 3  | テキスト入力 → `Ctrl + Enter` で FiveM VC に TTS 音声が流れることを確認                         |

### プレイ中のおすすめ操作フロー

1. アプリを **コンパクトモード**（`▼ 詳細` を畳んでウィンドウを小さく）にし、`Always on Top` でゲーム画面の隅に固定。
3. 発言したいときは **`Alt + Tab`** でアプリに切替え。
4. テキストを入力 → **`Ctrl + Enter`** で即発話 → **`Alt + Tab`** でゲームに戻る。

チャット欄に文字を打つ感覚でテンポ良く VC 発言できます。

