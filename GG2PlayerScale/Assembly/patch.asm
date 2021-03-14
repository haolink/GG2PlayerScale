use64

camscale:
  ;General data
  dd 3F800000h ;Scale
  dd 00000000h ;Stored height
  dd 00000000h ;Doki doki enabled
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
  ;Camera rotation z and w value
  dd 00000000h
  dd 00000000h
  dd 00000000h
  dd 00000000h
  ;Doki doki horizontal camera mod
  dd 00000000h
  dd 00000000h
  dd 00000000h
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
   push rdx
   mov rax,[rcx]
   mov ebx,[rax-06D0h]
   cmp ebx,$006F0052
   jne camscale_skip
   mov rbx,0xDEADCAFEDEADCAFE  ;Identifier
   ;xor rdx,rdx
   mov eax,[rbx]
   mov [rbx+28h],eax
   mov eax,[rcx+00000164h]

;Sanity check on the height
   ;cmp eax,0x42a60000 ;doki doki camera
   ;je camscale_eax_is_doki_doki
   cmp eax,0x41A00000
   jg camscale_accept_eax
   cmp eax,0x41200000
   je camscale_sanity_load_backup_height_alt
   ;cmp eax,0x00000000 ;CG camera
   ;je camscale_dont_have_good_value
camscale_sanity_load_backup_height:
   mov eax,[rbx+04h] ;Check if the old value is still looking good
   jmp camscale_sanity_check_backup_height
camscale_sanity_load_backup_height_alt:
   mov eax,[rbx+0ch]
camscale_sanity_check_backup_height:
   ;cmp eax,0x42a60000 ;doki doki backup camera
   ;je camscale_eax_is_doki_doki
   cmp eax,0x40000000
   jg camscale_accept_eax
camscale_dont_have_good_value:
   mov eax,0x43160000 ;Don't have a good value, so I'll just use this one
   ;jmp camscale_accept_eax
camscale_eax_is_doki_doki:
   ;mov eax,0x42480000 ;Lower to 50
   ;mov eax,0x00000000 ;Disable camera adjustment
   ;mov edx,0x00000001
   ;jmp camscale_write_status
;Sanity check is done
camscale_accept_eax:
   mov [rbx+38h],eax
   movaps xmm0,[rbx+10h]
   subps xmm0,[rbx+20h]
   mulps xmm0,[rbx+30h]
   subps xmm1,xmm0
camscale_write_status:
   cmp eax,0x42dc0000
   je camscale_store_alt_height
   mov [rbx+04h],eax   
camscale_store_alt_height:
   mov [rbx+0ch],eax
camscale_store_size_matrix:
   mov [rbx+38h],eax
   ;mov [rbx+8h],edx
camscale_skip:
   pop rdx
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
   push r12
   push r13

   sub rsp, 0x20
   movdqu  dqword [rsp+0x00], xmm8
   movdqu  dqword [rsp+0x10], xmm9

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

   mov rax,[rcx+0188h]
   mov [rbx+0060h],rax
   xor rax,rax

livecamscale_adjust:
   mov eax,[rcx+0164h]
   cmp eax,0
   jne livecamfixedcam_prepareloop
checkfordokidoki:
   xor r13,r13
   mov r13d,[rbx+08h]
   cmp r13d,0x00000001 ;We're in Doki Doki Mode
   jne livecamscale_default_adjust
   mov r13,rbx
   add r13,0x60
   jmp livecamscale_fixed_adjust
livecamfixedcam_prepareloop:
   mov r13,0xCAFECAFEDEADDEAD
   movd xmm9,eax
   xor r12,r12
livecamfixedcam_loop:
   mov r12d,[r13]
   cmp r12d,0x00000000
   je livecamscale_default_adjust
   movd xmm8,r12d
   subss xmm8,xmm9
   movd r12d,xmm8
   and r12d,7FFFFFFFh
   cmp r12d,3F000000h
   jle livecamscale_fixed_adjust
   add r13,0x20
   jmp livecamfixedcam_loop   
livecamscale_fixed_adjust:
   addps xmm1,[r13+0010h]
   jmp livecamscale_skip
livecamscale_default_adjust:
   addps xmm1,[rbx+0040h]
livecamscale_skip:
   movdqu  xmm8, dqword [rsp+0x00]
   movdqu  xmm9, dqword [rsp+0x10]
   add rsp, 0x20

   pop r13
   pop r12
   pop rbx
   pop rax
   movaps [rbx+0010h],xmm1
   movaps xmm0,[rcx+01A0h]
   jmp near [rip]
   dq 'RETURN02' ;Identifier