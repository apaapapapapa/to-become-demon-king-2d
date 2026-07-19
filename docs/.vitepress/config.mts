import { defineConfig } from 'vitepress'

export default defineConfig({
  lang: 'ja-JP',
  title: 'To Become Demon King Knowledge Base',
  description: 'ゲーム設計・仕様・ストーリー・世界設定・データベース・開発判断を一元管理するKnowledge Base',
  cleanUrls: true,
  lastUpdated: true,
  themeConfig: {
    nav: [
      { text: 'ゲーム', link: '/game/' },
      { text: '設計', link: '/design/' },
      { text: '仕様', link: '/specifications/' },
      { text: 'ストーリー', link: '/story/' },
      { text: '世界', link: '/world/' },
      { text: 'データベース', link: '/database/' },
      { text: '開発', link: '/development/' },
      { text: 'ADR', link: '/decisions/' }
    ],
    sidebar: {
      '/game/': [
        { text: 'ゲーム', items: [
          { text: '概要', link: '/game/' },
          { text: 'ゲームビジョン', link: '/game/vision' }
        ] }
      ],
      '/design/': [
        { text: '設計', items: [
          { text: '概要', link: '/design/' },
          { text: 'アーキテクチャ', link: '/design/architecture' },
          { text: '技術設計', link: '/design/technical-design' }
        ] }
      ],
      '/specifications/': [
        { text: '仕様', items: [
          { text: '仕様書一覧', link: '/specifications/' },
          { text: '入力', link: '/specifications/input' },
          { text: '戦闘', link: '/specifications/combat' },
          { text: 'インタラクション', link: '/specifications/interaction' },
          { text: '成長', link: '/specifications/progression' },
          { text: 'セーブ', link: '/specifications/save' }
        ] }
      ],
      '/story/': [
        { text: 'ストーリー', items: [
          { text: '概要', link: '/story/' },
          { text: 'ストーリー骨格', link: '/story/overview' }
        ] }
      ],
      '/world/': [
        { text: '世界設定', items: [
          { text: '概要', link: '/world/' }
        ] }
      ],
      '/database/': [
        { text: 'ゲームデータベース', items: [
          { text: '概要', link: '/database/' },
          { text: 'モンスター', link: '/database/monsters/' },
          { text: '進化テーブル', link: '/database/evolutions/' },
          { text: 'アイテム', link: '/database/items/' },
          { text: 'スキル', link: '/database/skills/' }
        ] }
      ],
      '/development/': [
        { text: '開発', items: [
          { text: '概要', link: '/development/' },
          { text: 'ロードマップ', link: '/development/roadmap' },
          { text: 'ドキュメント規約', link: '/development/documentation-rules' },
          { text: 'AI開発ガイド', link: '/development/ai-development' }
        ] }
      ],
      '/decisions/': [
        { text: 'Architecture Decision Records', items: [
          { text: 'ADR一覧', link: '/decisions/' },
          { text: 'ADR-0001 Knowledge Baseを同一リポジトリで管理', link: '/decisions/ADR-0001-monorepo-knowledge-base' }
        ] }
      ]
    },
    socialLinks: [
      { icon: 'github', link: 'https://github.com/apaapapapapa/to-become-demon-king-2d' }
    ],
    search: {
      provider: 'local'
    },
    outline: {
      level: [2, 3],
      label: 'このページ'
    },
    docFooter: {
      prev: '前へ',
      next: '次へ'
    },
    lastUpdated: {
      text: '最終更新'
    }
  }
})
