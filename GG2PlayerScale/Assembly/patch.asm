use64

camscale:
  ;General data
  dd 3F800000h ;Scale
  dd 00000000h ;Stored height
  dd 00000000h
  dd 00000000h
  ;Base scale 1.0
  dd 00000000h
  dd 00000000h
  dd 3F800000h
  dd 00000000h
  ;Value to subtract
  dd 00000000h
  dd 00000000h
  dd 00000000h
  dd 00000000h
  ;Value to multiply it with
  dd 00000000h
  dd 00000000h
  dd 00000000h
  dd 00000000h
  ;Live height adjust
  dd 00000000h
  dd 00000000h
  dd 00000000h
  dd 00000000h
  ;Subtitle adjust
  dd 00000000h
  dd 00000000h
  dd 41F00000h
  dd 00000000h

;Some distance so the code segments are nicely separated, makes debugging in CheatEngine easier.
align 10h
dd 00000000h  
align 20h
db 'CAMUPDATE' ;Identifier
align 20h

;This code segment is being executed whenever the player warps.
;It fixates a scale and the game will adjust all overlays to the current height.
camscale_assembly_codebegin:
   movaps xmm1,[rcx+00000190h]

   push rax
   push rbx
   mov rax,[rcx]
   mov ebx,[rax-06D0h]
   cmp ebx,$006F0052
   jne camscale_skip
   mov rbx,0xDEADCAFEDEADCAFE  ;Identifier
   mov eax,[rbx]
   mov [rbx+28h],eax
   mov eax,[rcx+00000164h]

;Sanity check on the height
   cmp eax,0x41a00000
   jg camscale_accept_eax
   mov eax,[rbx+04h] ;Check if the old value is still looking good
   cmp eax,0x40000000
   jg camscale_accept_eax
   mov eax,0x43160000 ;Don't have a good value, so I'll just use this one
;Sanity check is done
camscale_accept_eax:
   mov [rbx+04h],eax
   mov [rbx+38h],eax
   movaps xmm0,[rbx+10h]
   subps xmm0,[rbx+20h]
   mulps xmm0,[rbx+30h]
   subps xmm1,xmm0
camscale_skip:
   pop rbx
   pop rax

   mov rax,rdx
   movaps xmm0,xmm1
   movss [rdx],xmm1
   jmp near [rip]
   dq 'RETURN01' ;Identifier

;Some distance so the code segments are nicely separated, makes debugging in CheatEngine easier.
align 10h
dd 00000000h
align 20h   
db 'LIVECAM' ;Identifier
align 20h

;This code segment is being executed multiple times per second while the game isn't pause.
;Here live scaling and subtitle adjustment will take place.
camscale_live_assembly_codebegin:
   movaps xmm1,[rcx+0190h]
   push rax
   push rbx
   mov rbx,0xDEADCAFEDEADCAFE  ;Identifier
   ;General checks that we're catching the camera and nothing else
   mov eax,[rcx+01BCh]
   cmp eax,00020011h
   jne livecamscale_skip   
   mov eax,[rcx+017Ch]
   cmp eax,3F800000h   
   jne livecamscale_skip
   mov eax,[rcx+0018h]
   cmp eax,00000065h
   je livecamscale_skip
   mov eax,[rcx+0118h]
   cmp eax,00000007h
   jne livecamscale_skip
   ;mov eax,[rcx+0100h]
   ;cmp eax,00000000h
   ;je livecamscale_skip
   mov eax,[rcx+01A8h]	
   cmp eax,3F800000h
   jne livecamscale_skip

livecamscale_adjust:
   addps xmm1,[rbx+0040h]
livecamscale_skip:
   pop rbx
   pop rax
   movaps [rbx+0010h],xmm1
   movaps xmm0,[rcx+01A0h]
   jmp near [rip]
   dq 'RETURN02' ;Identifier