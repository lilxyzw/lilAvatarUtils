import { defineConfig } from 'vitepress'

const langName = '/ja_JP';

export const ja_JP = defineConfig({
  lang: 'ja_JP',
  description: "様々なアバター編集補助ツールが入ったパッケージです。",
  themeConfig: {
    logo: '/images/logo.svg',
    nav: [
      { text: 'ホーム', link: langName + '/' },
      { text: 'ドキュメント', link: langName + '/docs/', activeMatch: '/docs/' }
    ],
    sidebar: [
      {
        text: 'ドキュメント',
        link: langName + '/docs/',
        collapsed: false,
        items: [
          { text: 'アニメーション', link: langName + '/docs/AnimationClipGUI' },
          { text: 'メインウィンドウ', link: langName + '/docs/AvatarUtils' },
          { text: 'ライティング', link: langName + '/docs/LightingTestGUI' },
          { text: 'マテリアル', link: langName + '/docs/MaterialsGUI' },
          { text: 'PBコライダー', link: langName + '/docs/PhysBoneCollidersGUI' },
          { text: 'PhysBone', link: langName + '/docs/PhysBonesGUI' },
          { text: 'レンダラー', link: langName + '/docs/RenderersGUI' },
          { text: 'テクスチャ', link: langName + '/docs/TexturesGUI' },
          { text: 'ツール', link: langName + '/docs/UtilsGUI' },
        ]
      },
    ],
    search: {
      provider: 'local',
      options: {
        locales: {
          ja_JP: {
            translations: {
              button: {
                buttonText: '検索',
                buttonAriaLabel: '検索'
              },
              modal: {
                displayDetails: '詳細リストを表示',
                resetButtonTitle: '検索条件を削除',
                backButtonTitle: '検索を閉じる',
                noResultsText: '見つかりませんでした。',
                footer: {
                  selectText: '選択',
                  selectKeyAriaLabel: 'エンター',
                  navigateText: '切り替え',
                  navigateUpKeyAriaLabel: '上矢印',
                  navigateDownKeyAriaLabel: '下矢印',
                  closeText: '閉じる',
                  closeKeyAriaLabel: 'エスケープ'
                }
              }
            }
          }
        }
      }
    },
    lastUpdated: {
      text: '最終更新',
      formatOptions: {
        dateStyle: 'full',
        timeStyle: 'medium'
      }
    }
  }
})
