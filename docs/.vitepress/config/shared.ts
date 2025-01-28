import { defineConfig } from 'vitepress'
import markdownItTwemoji from '../theme/md/markdownItTwemoji'
import markdownItImage from '../theme/md/markdownItImage'
import markdownItToc from '../theme/md/markdownItToc'
import markdownItMDinMD from '../theme/md/markdownItMDinMD'

export const shared = defineConfig({
  base: '/lilAvatarUtils/',
  title: "lilAvatarUtils",
  head: [
    //['link', {rel: 'icon', type: 'image/svg+xml', href: '/lilAvatarUtils/images/logo.svg'}],
    ['meta', {property: 'og:type', content: 'website'}],
    //['meta', {property: 'og:image', content: 'https://lilxyzw.github.io/lilAvatarUtils/images/ogimg.jpg'}],
    ['meta', {property: 'twitter:card', content: 'summary'}],
    ['link', {rel: 'preconnect', href: 'https://fonts.googleapis.com'}],
    ['link', {rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: ''}],
    ['link', {href: 'https://fonts.googleapis.com/css2?family=Noto+Sans+JP:wght@100..900&family=Noto+Sans+Mono:wght@500&display=swap', rel: 'stylesheet'}],
  ],
  themeConfig: {
    //logo: '/images/logo.svg',
    socialLinks: [
      { icon: 'github', link: 'https://github.com/lilxyzw/lilAvatarUtils' }
    ],
    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright @ 2024-present lilxyzw'
    },
    // ドキュメントは自動生成なので編集ボタンは表示しない
    //editLink: {
    //  pattern: 'https://github.com/lilxyzw/lilAvatarUtils/edit/docs/docs/:path',
    //  text: 'Edit this page on GitHub'
    //},
    search: {
      provider: 'local'
    }
  },
  lastUpdated: true,
  markdown: {
    config: (md) => {
      md.use(markdownItTwemoji)
      md.use(markdownItImage)
      md.use(markdownItToc)
      md.use(markdownItMDinMD)
    }
  }
})
