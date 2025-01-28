import { defineConfig } from 'vitepress'
import { shared } from './shared'
import { en_US } from './en_US'
import { ja_JP } from './ja_JP'

export default defineConfig({
  ...shared,
  locales: {
    en_US: { label: 'English', ...en_US },
    ja_JP: { label: '日本語', ...ja_JP },
  }
})
