import { nextTick, onUnmounted, type Ref, watch } from 'vue'

export function useModalLifecycle(
  open: Ref<boolean>,
  close: () => void,
  initialFocus?: Ref<HTMLElement | null>,
) {
  let previousOverflow = ''

  function onKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      close()
    }
  }

  watch(
    open,
    async (isOpen) => {
      if (isOpen) {
        previousOverflow = document.body.style.overflow
        document.body.style.overflow = 'hidden'
        document.addEventListener('keydown', onKeydown)
        await nextTick()
        initialFocus?.value?.focus()
      } else {
        document.body.style.overflow = previousOverflow
        document.removeEventListener('keydown', onKeydown)
      }
    },
    { immediate: true },
  )

  onUnmounted(() => {
    document.body.style.overflow = previousOverflow
    document.removeEventListener('keydown', onKeydown)
  })
}
