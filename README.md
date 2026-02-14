# THIRD SDK For Unity

## Installation
THIRD SDK を導入する場合は、Unity Package Manager の **Install a Package from a Git URL** を使用してください。
手順は公式ドキュメント [Install a Package from a Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html) を参照してください。

### 1) この SDK 本体を導入（推奨: タグ固定）
- 推奨（実在するタグに固定。`<tag>` は実在タグに置き換えてください）

```text
https://github.com/UNCHAIN-RAY-PROJECT/sneo-unity-sdk.git?path=/Packages/ThirdSdk#<tag>
```

- タグ未作成時の暫定例（開発中のみ）

```text
https://github.com/UNCHAIN-RAY-PROJECT/sneo-unity-sdk.git?path=/Packages/ThirdSdk#main
```

> 本番利用では再現性のため、`#main` ではなく `#vX.Y.Z` のようなタグ固定を推奨します。

### 2) 必須依存 `com.mikeschweitzer.websocket` を導入
`ThirdConnector` は `com.mikeschweitzer.websocket` に依存しています。
**この依存を追加しない場合、ThirdConnector はコンパイルできません。**

```text
https://github.com/mikerochip/unity-websocket.git#1577a50ab7348d1a6fc4b320a3393e0e135b0f5f
```

### 3) `manifest.json` の dependencies 記述例
`Packages/manifest.json` の `dependencies` に、SDK 本体と必須依存を追加してください。

```json
{
  "dependencies": {
    "com.mikeschweitzer.websocket": "https://github.com/mikerochip/unity-websocket.git#1577a50ab7348d1a6fc4b320a3393e0e135b0f5f",
    "xyz.ooo-unchain-ooo.third-sdk": "https://github.com/UNCHAIN-RAY-PROJECT/sneo-unity-sdk.git?path=/Packages/ThirdSdk#<tag>"
  }
}
```

## Samples
See official instructions for how to [Import Samples from the Package](https://docs.unity3d.com/ja/current/Manual/upm-ui-details.html).
