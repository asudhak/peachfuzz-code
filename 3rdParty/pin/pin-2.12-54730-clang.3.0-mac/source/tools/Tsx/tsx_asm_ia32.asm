.686
.xmm
.model flat,c

ASSUME NOTHING

.CODE 
 ALIGN 4 
 XbeginAsm PROC
         mov eax,    1
         BYTE 0C7h
         BYTE 0F8h
         BYTE 2h
         BYTE 0h
         BYTE 0h
         BYTE 0h
        jmp   L2
abortLabel:
        mov   eax,  0
    L2:
        ret
XbeginAsm ENDP


XendAsm PROC
         BYTE 00fh
         BYTE 001h
         BYTE 0d5h
        ret
XendAsm ENDP

END