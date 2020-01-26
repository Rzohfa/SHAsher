.data									; data segment
										; global static uint32 K[]
	K dd 0428a2f98h, 071374491h, 0b5c0fbcfh, 0e9b5dba5h, 03956c25bh, 059f111f1h, 0923f82a4h, 0ab1c5ed5h, 0d807aa98h, 012835b01h, 0243185beh, 0550c7dc3h, 072be5d74h, 080deb1feh 
	  dd 09bdc06a7h, 0c19bf174h, 0e49b69c1h, 0efbe4786h, 00fc19dc6h, 0240ca1cch, 02de92c6fh, 04a7484aah, 05cb0a9dch, 076f988dah, 0983e5152h, 0a831c66dh, 0b00327c8h, 0bf597fc7h
	  dd 0c6e00bf3h, 0d5a79147h, 006ca6351h, 014292967h, 027b70a85h, 02e1b2138h, 04d2c6dfch, 053380d13h, 0650a7354h, 0766a0abbh, 081c2c92eh, 092722c85h, 0a2bfe8a1h, 0a81a664bh
	  dd 0c24b8b70h, 0c76c51a3h, 0d192e819h, 0d6990624h, 0f40e3585h, 0106aa070h, 019a4c116h, 01e376c08h, 02748774ch, 034b0bcb5h, 0391c0cb3h, 04ed8aa4ah, 05b9cca4fh, 0682e6ff3h
	  dd 0748f82eeh, 078a5636fh, 084c87814h, 08cc70208h, 090befffah, 0a4506cebh, 0bef9a3f7h, 0c67178f2h
	Kaddr dq offset K 					; address to K array
.code									; code segment
hashAsm PROC 							; void hash(uint64 bytes [rcx], int32 N [rdx], uint64 OutputBuff [r8])
	push rbp							; saving previous base pointer
	mov rbp, rsp						; assign stack pointer as current base pointer
	sub rsp, 356						; reserve place for local variables and arrays
	lea rax, [rbp-336]					; get address of W array
	mov [rbp-356], rax					; save address of W array
	mov [rbp+16], rcx					; save first function argument on stack
	mov [rbp+24], rdx					; save second function argument on stack
	mov [rbp+28], r8					; save third function argument on stack
	mov rax, [rbp+16]					; get input array address
	mov [rbp-348], rax					; M = bytes
	mov rax, [rbp+24]					; get N
	mov [rbp-340], eax					; save N to local variable
	sfence								; Ensuring correct load order
	mov dword ptr [rbp-52], 06a09e667h	; init Hash array values H[0]
	mov dword ptr [rbp-56], 0bb67ae85h	; init Hash array values H[1]
	mov dword ptr [rbp-60], 03c6ef372h	; init Hash array values H[2]
	mov dword ptr [rbp-64], 0a54ff53ah	; init Hash array values H[3]
	mov dword ptr [rbp-68], 0510e527fh	; init Hash array values H[4]
	mov dword ptr [rbp-72], 09b05688ch	; init Hash array values H[5]
	mov dword ptr [rbp-76], 01f83d9abh	; init Hash array values H[6]
	mov dword ptr [rbp-80], 05be0cd19h	; init Hash array values H[7]
	sfence								; Ensuring correct load order
	mov dword ptr [rbp-44], 0			; int i = 0
	loop1:								; for loop
		mov eax, [rbp-44]				; i -> eax
		cmp eax, [rbp-340]				; if i >= N
		jge loop1end					; end loop
		mov dword ptr [rbp-48], 0		; int t = 0
		loop2:							; for loop
			cmp dword ptr [rbp-48], 16	; if t >= 16
			jge loop2end				; end loop
			mov eax, [rbp-44]			; i -> eax
			shl eax, 4					; i * 16
			add eax, [rbp-48]			; ^ + t
			shl eax, 2					; (i*16+t) << 4
			add rax, [rbp-348]			; M[i*16+t]
			mov ebx, [rax]				; M[i*16+t] -> ebx
			mov eax, [rbp-48]			; t -> eax
			shl eax, 2					; eax << 2
			add rax, [rbp-356]			; get W[t] address
			mov dword ptr [rax], ebx	; W[t] = M[i*16+t]
			inc dword ptr [rbp-48]		; t++ 
			jmp loop2					; goto for loop
		loop2end:						; end of for loop
		loop3:							; for loop
			cmp dword ptr [rbp-48], 64	; if t >= 16
			jge loop3end				; end loop
			 
			mov eax, dword ptr [rbp-48]	; get t
			sub rax, 2					; get t-2
			shl rax, 2					; t-2 << 2
			add rax, [rbp-356]			; get address W[t-2]
			mov ebx, [rax]				; get W[t-2] value
			sub rsp, 4					; make room on stack for function argument
			mov [rsp], ebx				; move function argument to stack
			call S_SIG1					; small_sigma1(W[t-2])
			add rsp, 4					; clear stack
			mov [rbp-40], eax			; move returned value to local variable
 
			mov eax, dword ptr [rbp-48]	; get t
			sub rax, 7					; get t-7
			shl rax, 2					; t-2 << 2
			add rax, [rbp-356]			; get address W[t-7]
			mov eax, dword ptr [rax]	; get value W[t-7]
			add dword ptr [rbp-40], eax	; add W[t-7] to local variable
										
			mov eax, dword ptr [rbp-48]	; get t
			sub rax, 15					; get t-15
			shl rax, 2					; t-15 << 2
			add rax, [rbp-356]			; get address W[t-215]
			mov ebx, [rax]				; get W[t-15] value
			sub rsp, 4					; make room on stack for function argument
			mov [rsp], ebx				; move function argument to stack
			call S_SIG0					; small_sigma0(W[t-2])
			add rsp, 4					; clear stack
			add dword ptr [rbp-40], eax	; add returned value to local variable
			 
			mov eax, dword ptr [rbp-48]	; get t
			sub rax, 16					; get t-7
			shl rax, 2					; t-2 << 2
			add rax, [rbp-356]			; get address W[t-7]
			mov eax, dword ptr [rax]	; get value W[t-7]
			add dword ptr [rbp-40], eax	; add W[t-7] to local variable
 
 			mov eax, dword ptr [rbp-48]	; get t
			shl rax, 2					; t-2 << 2
			add rax, [rbp-356]			; get address W[t]
			mov ebx, dword ptr [rbp-40]	; get local variable small_sigma1(W[t-2]) + W[t-7] + small_sigma0(W[t-15]) + W[t-16]
			mov dword ptr [rax], ebx	; W[t] = small_sigma1(W[t-2]) + W[t-7] + small_sigma0(W[t-15]) + W[t-16]
 
			inc dword ptr [rbp-48]		; t++ 
			jmp loop3					; goto for loop
		loop3end:						; end of loop
		mov rcx, 32						; 4*8 = 32 -> rcx
		lea rdi, [rbp-40]				; get h address
		lea rsi, [rbp-80]				; get H[7] address
		rep movsb						; a = H[0] ... h = H[7]
		mov dword ptr [rbp-48], 0		; int t = 0
		loop4:							; for loop
			cmp dword ptr [rbp-48], 64	; if t >= 64
			jge loop4end				; end loop
			; Calculate T1
			mov eax, [rbp-40]			; get h
			mov [rbp-4], eax			; T1 = h
			mov eax, [rbp-28]			; get e
			sub rsp, 4					; make space on stack
			mov [rsp], eax				; save argument e on stack
			call B_SIG1					; big_sigma1(e)
			add rsp, 4					; clear stack
			add [rbp-4], eax			; T1 += big_sigma(e)
			sub rsp, 12					; make space on stack
			mov eax, [rbp-28]			; get e
			mov [rsp], eax				; save argument e on stack
			mov eax, [rbp-32]			; get f
			mov [rsp+4], eax			; save argument f on stack
			mov eax, [rbp-36]			; get g
			mov [rsp+8], eax			; save argument g on stack
			call ChF					; Ch(e, f, g)
			add rsp, 12					; clear stack
			add [rbp-4], eax			; T1 += Ch(e, f, g)
			mov eax, dword ptr [rbp-48]	; get t
			shl rax, 2					; get t << 2
			add rax, [Kaddr]			; get address K[t]
			mov ebx, [rax]				; get value K[t]
			add [rbp-4], ebx			; T1 += K[t]
			mov eax, dword ptr [rbp-48]	; get t
			shl rax, 2					; get t << 2
			add rax, [rbp-356]			; get address W[t]
			mov ebx, [rax]				; get value W[t]
			add [rbp-4], ebx			; T1 += W[t]
			; Calculate T2	
			mov eax, [rbp-12]			; get a
			sub rsp, 4					; make space on stack
			mov [rsp], eax				; save argument a on stack
			call B_SIG0					; big_sigma(a)
			add rsp, 4					; clear stack
			mov [rbp-8], eax			; T2 = big_sigma(a)
			sub rsp, 12					; make space on stack
			mov eax, [rbp-12]			; get a
			mov [rsp], eax				; save argument a on stack
			mov eax, [rbp-16]			; get b
			mov [rsp+4], eax			; save argument b on stack
			mov eax, [rbp-20]			; get c
			mov [rsp+8], eax			; save argument c on stack
			call MajF					; Maj(a, b, c)
			add rsp, 12					; clear stack
			add [rbp-8], eax			; T2 += Maj(a, b, c)

			mov rcx, 28					; 4*7 = 28
			lea rdi, [rbp-40]			; get h address
			lea rsi, [rbp-36]			; get g address
			rep movsb					; h = g ... b = a
			mov eax, [rbp-4]			; get T1
			add [rbp-28], eax			; e += T1
			add eax, [rbp-8]			; T1 += T2
			mov [rbp-12], eax			; a = T1+T2
			  
			inc dword ptr [rbp-48]		; t++
			jmp loop4					; goto for loop
		loop4end:						; end of loop

		mov eax, [rbp-40]				; get h
		add [rbp-80], eax				; H[7] += h
		mov eax, [rbp-36]				; get g
		add [rbp-80+4], eax				; H[6] += g
		mov eax, [rbp-32]				; get f
		add [rbp-80+8], eax				; H[5] += f
		mov eax, [rbp-28]				; get e
		add [rbp-80+12], eax			; H[4] += e
		mov eax, [rbp-24]				; get d
		add [rbp-80+16], eax			; H[3] += d
		mov eax, [rbp-20]				; get c
		add [rbp-80+20], eax			; H[2] += c
		mov eax, [rbp-16]				; get b
		add [rbp-80+24], eax			; H[1] += b
		mov eax, [rbp-12]				; get a
		add [rbp-80+28], eax			; H[0] += a
 
		inc dword ptr [rbp-44]			; i++
		jmp loop1						; goto for loop
	loop1end:							; end of loop

	sfence								; Ensuring correct load order
	mov eax, [rbp-52]					; get H[0]
	mov rbx, [rbp+28]					; get output buffer address
	mov [rbx], eax						; Out[0] = H[0]
	mov eax, [rbp-56]					; get H[1]
	mov [rbx+4], eax					; Out[1] = H[1]
	mov eax, [rbp-60]					; get H[2]
	mov [rbx+8], eax					; Out[2] = H[2]
	mov eax, [rbp-64]					; get H[3]
	mov [rbx+12], eax					; Out[3] = H[3]
	mov eax, [rbp-68]					; get H[4]
	mov [rbx+16], eax					; Out[4] = H[4]
	mov eax, [rbp-72]					; get H[5]
	mov [rbx+20], eax					; Out[5] = H[5]
	mov eax, [rbp-76]					; get H[6]
	mov [rbx+24], eax					; Out[6] = H[6]
	mov eax, [rbp-80]					; get H[7]
	mov [rbx+28], eax					; Out[7] = H[7]
	sfence								; Ensuring correct load order

	mov rsp, rbp						; restore stack pointer
	pop rbp								; restore base pointer
hashAsm ENDP							; END OF HASHER FUNCTION


ChF PROC 								; uint32 Ch(uint32 x, uint32 y, uint32 z)
	push rbp							; save base pointer
	mov rbp, rsp						; assign stack pointer as current base pointer
	sub rsp, 4							; make space on stack
	mov eax, [rbp+16]					; get x
	and eax, [rbp+20]					; and x&y
	mov [rbp-4], eax					; save x&y
	mov eax, [rbp+16]					; get x
	not eax								; not ~x
	and eax, [rbp+24]					; and ~x&z
	xor eax, [rbp-4]					; xor x&y ^ ~x&z
	mov rsp, rbp						; restore stack pointer
	pop rbp								; restore base pointer
	ret									; return from function
ChF ENDP								; END OF Ch FUNCTION


MajF PROC 								; uint32 Maj(uint32 x, uint32 y, uint32 z)
	push rbp							; save base pointer
	mov rbp, rsp						; assign stack pointer as current base pointer
	sub rsp, 4							; make space on stack
	mov eax, [rbp+16]					; get x
	and eax, [rbp+20]					; get x&y
	mov [rbp-4], eax					; save x&y
	mov eax, [rbp+16]					; get x
	and eax, [rbp+24]					; and x&z
	xor [rbp-4], eax					; xor x&y ^ x&z
	mov eax, [rbp+20]					; get y
	and eax, [rbp+24]					; and y&z
	xor eax, [rbp-4]					; xor x&y ^ x&z ^ y&z
	mov rsp, rbp						; restore stack pointer
	pop rbp								; restore base pointer
	ret									; return from function
MajF ENDP								; END OF Maj FUNCTION


S_SIG0 PROC								; int32 small_sigma0(int32 x)
	push rbp							; save base pointer
	mov rbp, rsp						; assign stack pointer as current base pointer
	sub rsp, 4							; make space on stack
	mov eax, [rbp+16]					; get x
	ror eax, 7							; rotate_right(x,7)
	mov [rbp-4], eax					; save rotate_right(x,7)
	ror eax, 11							; rotate_right(x,7+11)
	xor [rbp-4], eax					; xor rotate_right(x,7) ^ rotate_right(x,7+11)
	mov eax, [rbp+16]					; get x
	shr eax, 3							; shift right x >> 3
	xor eax, [rbp-4]					; xor rotate_right(x,7) ^ rotate_right(x,7+11) ^ (x>>3)
	mov rsp, rbp						; restore stack pointer
	pop rbp								; restore base pointer
	ret									; return from function
S_SIG0 ENDP								; END OF small_sigma0 FUNCTION


S_SIG1 PROC								; int32 small_sigma1(int32 x)
	push rbp							; save base pointer
	mov rbp, rsp						; assign stack pointer as current base pointer
	sub rsp, 4							; make space on stack
	mov eax, [rbp+16]					; get x
	ror eax, 17							; rotate_right(x,17)
	mov [rbp-4], eax					; save rotate_right(x,17)
	ror eax, 2							; rotate_right(x,17+2)
	xor [rbp-4], eax					; xor rotate_right(x,17) ^ rotate_right(x,17+2)
	mov eax, [rbp+16]					; get x
	shr eax, 10							; shift right x >> 10
	xor eax, [rbp-4]					; xor rotate_right(x,17) ^ rotate_right(x,17+2) ^ (x>>10)
	mov rsp, rbp						; restore stack pointer
	pop rbp								; restore base pointer
	ret									; return from function
S_SIG1 ENDP								; END OF small_sigma1 FUNCTION


B_SIG0 PROC								; int32 big_sigma0(int32 x)
	push rbp							; save base pointer
	mov rbp, rsp						; assign stack pointer as current base pointer
	sub rsp, 4							; make space on stack
	mov eax, [rbp+16]					; get x
	ror eax, 2							; rotate_right(x,2)
	mov [rbp-4], eax					; save rotate_right(x,2)
	ror eax, 11							; rotate_right(x,2+11)
	xor [rbp-4], eax					; xor rotate_right(x,2) ^ rotate_right(x,2+11)
	ror eax, 9							; rotate_right(x,2+11+9)
	xor eax, [rbp-4]					; xor rotate_right(x,2) ^ rotate_right(x,2+11) ^ rotate_right(x,2+11+9)
	mov rsp, rbp						; restore stack pointer
	pop rbp								; restore base pointer
	ret									; return from function
B_SIG0 ENDP								; END OF big_sigma0 FUNCTION


B_SIG1 PROC								; int32 big_sigma1(int32 x)
	push rbp							; save base pointer
	mov rbp, rsp						; assign stack pointer as current base pointer
	sub rsp, 4							; make space on stack
	mov eax, [rbp+16]					; get x
	ror eax, 6							; rotate_right(x,6)
	mov [rbp-4], eax					; save rotate_right(x,6)
	ror eax, 5							; rotate_right(x,6+5)
	xor [rbp-4], eax					; xor rotate_right(x,6) ^ rotate_right(x,6+5)
	ror eax, 14							; rotate_right(x,6+5+14)
	xor eax, [rbp-4]					; xor rotate_right(x,6) ^ rotate_right(x,6+5) ^ rotate_right(x,6+5+14)
	mov rsp, rbp						; restore stack pointer
	pop rbp								; restore base pointer
	ret									; return from function
B_SIG1 ENDP								; END OF big_sigma1 FUNCTION


end										; END OF DLL 