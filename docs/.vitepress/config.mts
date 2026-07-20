import { defineConfig } from 'vitepress'

export default defineConfig({
  base: '/to-become-demon-king-2d/',
  lang: 'ja-JP',
  title: 'To Become Demon King Knowledge Base',
  description: 'ゲーム設計・仕様・ストーリー・世界設定・図鑑・開発判断を一元管理するKnowledge Base',
  cleanUrls: true,
  lastUpdated: true,
  themeConfig: {
    nav: [
      { text: 'ゲーム', link: '/game/' },
      { text: '設計', link: '/design/' },
      { text: '仕様', link: '/specifications/' },
      { text: 'ストーリー', link: '/story/' },
      { text: '世界', link: '/world/' },
      { text: '図鑑', link: '/database/' },
      { text: '開発', link: '/development/' },
      { text: 'ADR', link: '/decisions/' }
    ],
    sidebar: {
      '/game/': [{ text: 'ゲーム', items: [
        { text: '概要', link: '/game/' },
        { text: 'ゲームビジョン', link: '/game/vision' }
      ] }],
      '/design/': [{ text: '設計', items: [
        { text: '概要', link: '/design/' },
        { text: 'アーキテクチャ', link: '/design/architecture' },
        { text: '技術設計', link: '/design/technical-design' },
        { text: 'Feature間の責務境界', link: '/design/feature-boundaries' }
      ] }],
      '/specifications/': [{ text: '仕様', items: [
        { text: '仕様書一覧', link: '/specifications/' },
        { text: '入力', link: '/specifications/input' },
        { text: '移動', link: '/specifications/movement' },
        { text: 'Interaction', link: '/specifications/interaction' },
        { text: 'Dialogue', link: '/specifications/dialogue' },
        { text: 'Ability', link: '/specifications/ability' },
        { text: '戦闘', link: '/specifications/combat' },
        { text: 'Art', link: '/specifications/art' },
        { text: 'Skill', link: '/specifications/skill' },
        { text: 'Evolution', link: '/specifications/evolution' },
        { text: '成長', link: '/specifications/progression' },
        { text: 'Quest', link: '/specifications/quest' },
        { text: 'Spawning', link: '/specifications/spawning' },
        { text: 'セーブ', link: '/specifications/save' }
      ] }],
      '/story/': [{ text: 'ストーリー', items: [
        { text: '概要', link: '/story/' },
        { text: 'ストーリー骨格', link: '/story/overview' }
      ] }],
      '/world/': [{ text: '世界設定', items: [
        { text: '概要', link: '/world/' }
      ] }],
      '/database/': [{ text: '図鑑', items: [
        { text: '図鑑一覧', link: '/database/' },
        { text: 'モンスター', link: '/database/monsters/' },
        { text: 'Ability', link: '/database/abilities/' },
        { text: 'Art', link: '/database/arts/' },
        { text: 'Skill', link: '/database/skills/' },
        { text: 'Evolution', link: '/database/evolutions/' },
        { text: 'アイテム', link: '/database/items/' }
      ] }],
      '/development/': [{ text: '開発', items: [
        { text: '概要', link: '/development/' },
        { text: 'ロードマップ', link: '/development/roadmap' },
        { text: 'リリース運用', link: '/development/release' },
        { text: 'ドキュメント規約', link: '/development/documentation-rules' }
      ] }],
      '/decisions/': [{ text: 'Architecture Decision Records', items: [
        { text: 'ADR一覧', link: '/decisions/' },
        { text: 'ADR-0001 Knowledge Baseを同一リポジトリで管理', link: '/decisions/ADR-0001-monorepo-knowledge-base' },
        { text: 'ADR-0002 Ability・Art・Skillの責務分離', link: '/decisions/ADR-0002-ability-art-skill-boundaries' },
        { text: 'ADR-0003 Evolution Nodeと排他経路', link: '/decisions/ADR-0003-evolution-nodes-and-exclusive-paths' }
      ] }]
    },
    socialLinks: [
      { icon: 'github', link: 'https://github.com/apaapapapapa/to-become-demon-king-2d' }
    ],
    search: { provider: 'local' },
    outline: { level: [2, 3], label: 'このページ' },
    docFooter: { prev: '前へ', next: '次へ' },
    lastUpdated: { text: '最終更新' }
  }
})
