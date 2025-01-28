import { defineConfig } from 'vitepress'

const langName = '/en_US';

export const en_US = defineConfig({
  lang: 'en_US',
  description: "This is a package containing various avatar editing assistance tools.",
  themeConfig: {
    logo: '/images/logo.svg',
    nav: [
      { text: 'Home', link: langName + '/' },
      { text: 'Document', link: langName + '/docs/', activeMatch: '/docs/' }
    ],
    sidebar: [
      {
        text: 'Document',
        link: langName + '/docs/',
        collapsed: false,
        items: [
          { text: 'Animations', link: langName + '/docs/AnimationClipGUI' },
          { text: 'Main Window', link: langName + '/docs/AvatarUtils' },
          { text: 'Lighting', link: langName + '/docs/LightingTestGUI' },
          { text: 'Materials', link: langName + '/docs/MaterialsGUI' },
          { text: 'PBColliders', link: langName + '/docs/PhysBoneCollidersGUI' },
          { text: 'PhysBones', link: langName + '/docs/PhysBonesGUI' },
          { text: 'Renderers', link: langName + '/docs/RenderersGUI' },
          { text: 'Textures', link: langName + '/docs/TexturesGUI' },
          { text: 'Utils', link: langName + '/docs/UtilsGUI' },
        ]
      },
    ],
    search: {
      provider: 'local',
      options: {
        locales: {
          en_US: {
            translations: {
              button: {
                buttonText: 'Search',
                buttonAriaLabel: 'Search'
              },
              modal: {
                displayDetails: 'Display detailed list',
                resetButtonTitle: 'Reset search',
                backButtonTitle: 'Close search',
                noResultsText: 'No results for',
                footer: {
                  selectText: 'to select',
                  selectKeyAriaLabel: 'enter',
                  navigateText: 'to navigate',
                  navigateUpKeyAriaLabel: 'up arrow',
                  navigateDownKeyAriaLabel: 'down arrow',
                  closeText: 'to close',
                  closeKeyAriaLabel: 'escape'
                }
              }
            }
          }
        }
      }
    },
    lastUpdated: {
      text: 'Updated at',
      formatOptions: {
        dateStyle: 'full',
        timeStyle: 'medium'
      }
    }
  }
})
