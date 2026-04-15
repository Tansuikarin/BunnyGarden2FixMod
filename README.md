# BunnyGarden2FixMod

[バニーガーデン2](https://store.steampowered.com/app/3443820/2/)(海外名:Bunny Garden2)用の解像度修正やフレームレート上限変更などを行うBepInEx5用Modです。

## 対応バージョン(v1.0.0現在)
- ゲームバージョン1.0.1のみ対応  

## 機能
- 内部解像度を指定することで画質を向上することができる。
- 本来は60で固定されていたフレームレート制限を任意の値にするか、取り払うことができる。

## 導入方法
1. [Releases](https://github.com/kazumasa200/BunnyGarden2FixMod/releases)から最新のzipファイルをダウンロードする。(BunnyGarden2FixMod v1.0.0.zipみたいな感じ)ブラウザによってはブロックするかもしれないので注意。<br>導入時の最新バージョンを入れてください。
1. [BepInEx5](https://github.com/bepinex/bepinex/releases)をダウンロードする。Windowsの場合は```BepInEx_win_x64_{バージョン名}.zip```をダウンロードする。
1. ゲームのexeがあるディレクトリにBepInEx5の中身を展開。つまり、ゲームのexeとBepInExフォルダやdoorstop_configとかが同じ階層にある状態が正しいということ。

1. 一度ゲームを起動した後、[Releases](https://github.com/kazumasa200/BunnyGarden2FixMod/releases)からダウンロードしたZipを展開し、中にある```net.noeleve.BunnyGarden2FixMod.dll```をBepinExフォルダの中にPluginsの中に入れる。

1. もう一度起動するとBepinExフォルダの中のconfigフォルダに```net.noeleve.BunnyGarden2FixMod.cfg```設定ファイルが出来上がるので、それをメモ帳などで変更して解像度の設定やフレームレートなどの設定をする。

## 既知の問題点
[Issues](https://github.com/kazumasa200/BunnyGarden2FixMod/issues)をご確認ください。バグや改善点、ほしい機能ありましたら[Issues](https://github.com/kazumasa200/BunnyGarden2FixMod/issues)もしくは[X](https://x.com/kazumasa200)までお願いします。  
要望の際は右上のNew Issueから個別のissueを作ってください。

## お問い合わせ
X(旧Twitter):@kazumasa200
