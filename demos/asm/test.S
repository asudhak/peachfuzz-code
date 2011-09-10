
.text
.align 2

val:
 .long 10

# Multiply input value by 10...
.global mult10
.type mult10, function
mult10:
  movl val,%eax
  imul 4(%esp),%eax
  ret
