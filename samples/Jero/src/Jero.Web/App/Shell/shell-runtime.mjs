import { defineComponent, h } from 'vue'
import { RouterView, useRoute, useRouter } from 'vue-router'
import { ElConfigProvider } from 'element-plus'
import en from 'element-plus/es/locale/lang/en'
import zhCn from 'element-plus/es/locale/lang/zh-cn'
import { LayoutDashboard, Settings, Building2, ScrollText, Terminal, BookOpen } from 'lucide-vue-next'
import { Create } from '@jero/runtime/components/shell-frame-state.js'
import { Create as CreateLock } from '@jero/runtime/components/lock-overlay-state.js'
import ShellFrame from './ShellFrame.jazor'
import LockOverlay from './LockOverlay.jazor'
import 'element-plus/dist/index.css'
import './shell.css'

const icons = [LayoutDashboard, Settings, Building2, ScrollText, Terminal, BookOpen]

const Lock = defineComponent({
    props: ['avatar', 'username', 'unlock', 'logout'],
    setup(props) {
        const state = CreateLock()
        state.Setup(props.unlock)
        return () => h(LockOverlay, state.CreateProps(props.avatar, props.username, props.logout))
    },
})

export default defineComponent({
    setup() {
        const route = useRoute()
        const router = useRouter()
        const frame = Create({
            Path: () => route.path,
            Push: path => router.push(path),
            Replace: path => router.replace(path),
            Resolve: path => router.resolve(path).href,
        })
        const state = frame.State
        frame.Setup()

        return () => h(ElConfigProvider, { locale: frame.Lang === 'en' ? en : zhCn }, {
            default: () => h(ShellFrame, frame.CreateProps(String(route.meta.title ?? '')), {
                // A query change remounts the matched route component.
                route: () => h(RouterView, {}, { default: ({ Component }) => Component && h(Component, { key: route.fullPath }) }),
                lock: () => frame.Locked && h(Lock, {
                    avatar: state.User?.Avatar ?? '',
                    username: frame.Username(),
                    unlock: () => frame.Unlock(),
                    logout: () => frame.Logout(),
                }),
                tabIcon: ({ path }) => {
                    const icon = icons[frame.TabIconIndex(path)]
                    return icon && h(icon, { size: 13 })
                },
                sectionIcon: ({ title, initial }) => {
                    const icon = icons[frame.IconIndex(title)]
                    return icon ? h(icon, { size: 18 }) : h('span', { class: 'jero-shell__rail-glyph' }, initial)
                },
            }),
        })
    },
})
