# MinimumAsyncBridge

async/await の実行に必要な最低限の実装。

C# 5.0 の async/await を .NET 3.5/Unity ゲーム エンジン上で実行できるようにするためのライブラリ。

## 背景

### C# 5.0 async/await

C# 5.0 の async/await の実行には、

- .NET 4 で追加された 'Task' クラス
- .NET 4.5 で追加された `AsyncTaskMethodBuilder` などの一連のクラス

が必要。

これらをバックポーティングすれば、.NET 3.5 上でも C# 5.0 が使える。

### Unity の制限

ただ、Unity は利用している .NET ランタイムが古くて(Mono 2.8系)、いろいろ制限がある。

特に、iOS は仮想マシン実行ができないので AOT (Ahead of Time)コンパイル実行していて、これの制限が特に厳しい。

2015年現在、Unity は iOS 上での実行を Mono の AOT コンパイルから、独自実装の [IL2CPP](http://blogs.unity3d.com/2014/05/20/the-future-of-scripting-in-unity/)という方式に移行中なものの、
これもまだまだ不安定。

結果として、フル機能の `Task` の移植は Unity 上(特に iOS)で動かせない可能性が高い。

実際、有名どころの `Task` バックポーティングを試しにUnity上で使ってみたところ、現状ではIL2CPPのコンパイルに失敗(2015/7時点)。

- [AsyncBridge](http://omermor.github.io/AsyncBridge/)

#### 将来的に

IL2CPP は徐々に改善していっているので、上記制限は徐々になくなるはず。

ただ、IL2CPP はコンパイルに非常に時間がかかるので、実機確認のたびに IL2CPP でコンパイルするのはストレスが多そう。

なので、当面の間は Mono AOT でも動くライブラリが必要。
かつ、もしかしたら将来的には標準ライブラリに移行できる可能性も考えた実装が必要。

## このリポジトリが提供するもの

async/await を使いたいだけなら、フル機能の `Task` クラスは要らない。
必要なのは `TaskCompletionSource<TResult>` クラス(C++ の `std::promise` 的なもの)だけなので、ここだけ実装すればいい。

ここだけであれば、Mono AOT や IL2CPP の制限に引っかかることなく実装できた。

残りの、一般的な `Task` クラスの債務、すなわち、マルチスレッド実行(`Task.Run`)やタイマー(`Task.Delay`)は実装していない。
この辺りは、他の非同期処理ライブラリとつなぎこんで埋める想定。

具体的には、このリポジトリには、

- 標準の `Task` の下位互換実装
  - `System.Threading.Tasks` 名前空間に定義
  - `TaskCompletionSource<TResult>` に関連する部分だけを実装
    - `Task.Run`や`Task.Delay`などのメソッドはない。
  - .NET 4.5以上で実行する用に、標準ライブラリへの型フォワーディングも提供
- `AsyncTaskMethodBuilder` のバックポーティング
  - [マイクロソフトの参照ソースコード](http://referencesource.microsoft.com/)がベース
- [UniRx](https://github.com/neuecc/UniRx)と[IteratorTasks](https://github.com/OrangeCube/IteratorTasks)に対する awaiter 実装

が含まれている。

## バイナリ提供

NuGet パッケージ化済み:

- [MinimumAsyncBridge](https://www.nuget.org/packages/MinimumAsyncBridge/): async/await の実行に必要な最低限の実装
  - .NET 4.5 からパッケージ参照すると、標準ライブラリへの型フォワーディング
- [IteratorTasks.AsyncBridge](https://www.nuget.org/packages/IteratorTasks.AsyncBridge/): [IteratorTasks](https://www.nuget.org/packages/IteratorTasks/)に対する awaiter 実装

(※ [UniRx](https://github.com/neuecc/UniRx)は本体が NuGet パッケージになっていないので awaiter 実装のパッケージ公開なし)

