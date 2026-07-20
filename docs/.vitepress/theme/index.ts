import DefaultTheme from 'vitepress/theme'
import ContentCatalog from './ContentCatalog.vue'
import ContentRelations from './ContentRelations.vue'
import RuntimeContentHeader from './RuntimeContentHeader.vue'

export default {
  extends: DefaultTheme,
  enhanceApp({ app }) {
    app.component('ContentCatalog', ContentCatalog)
    app.component('ContentRelations', ContentRelations)
    app.component('RuntimeContentHeader', RuntimeContentHeader)
  }
}
