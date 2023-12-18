\ various utils

\ take an address and a number of bytes to read
\ the return the number represented across len bytes
: multiByteNum ( addr len -- num )
    0            \ (addr len acc -- )  we're initialzing the accumulator value
    rot rot      \ (acc addr len -- )  move the accumulator above the others
    0            \ (acc addr len 0 -- )
    swap         \ (acc addr 0 len -- )

    \ count down in the iterator, so that as we iterator through the buffer,
    \ the iterator I will indicate how many bits (8 * I) we need to shift by
    \ so that the final spot in the buffer is the final byte of the number
    -DO
        dup      \ (acc addr addr -- )
        c@       \ (acc addr valueAtOffsetAddr -- )
        I 1 - 8 *    \ (acc addr valueAtOffsetAddr 8i -- )
        lshift   \ (acc addr shiftedValueAtOffset -- )
        rot      \ (addr shiftedValueAtOffset acc -- )
        +        \ (addr updatedAcc -- )
        swap     \ (acc addr -- )
        1 +      \ (acc offsetaddr -- )
    1 -LOOP

    drop         \ (acc -- ) drop the addrs, and we'll leave the accumulator on the stack
;

\ take an address and a number of bytes to read
\ the return the number represented across len bytes
: writeMultiByteNum ( acc addr len -- )
    0               \ (acc addr len 0 -- )
    swap            \ (acc addr 0 len -- )

    \ count down in the iterator, so that as we iterator through the buffer,
    \ the iterator I will indicate how many bits (8 * I) we need to shift by
    \ so that the final spot in the buffer is the final byte of the number
    -DO
        \ DEBUG PRINTOUTS ." LOOP ITER " I . ." stack: " .s cr 
                    \ (acc addr -- ) start here
        swap        \ (addr acc -- )
        dup         \ (addr acc acc -- )
        I 1 - 8 *   \ (addr acc acc 8i -- )
        0xFF swap lshift
        and         \ (addr acc thisByte -- )
        I 1 - 8 * rshift
        rot         \ (acc thisByte addr -- )
        dup         \ (acc thisByte addr addr -- )
        rot         \ (acc addr addr thisByte -- )
        swap        \ (acc addr thisByte addr -- )
        \ DEBUG PRINTOUTS ."     GOING TO WRITE " 2dup swap hex. ." TO ADDR " hex. cr 
        c!          \ (acc addr -- )
        1 +         \ (acc offsetAddr -- )
    1 -LOOP

    \ not sure why this code was originally writing out one more byte, but that seems wrong.
    \ now just drop the final char and addr from the stack and move on
    drop drop 
;

\ take an address and a number of bytes to read
\ the return the number represented across len bytes
: multiByteVarInt ( addr len -- num )
    0            \ (addr len acc -- )  we're initialzing the accumulator value
    rot rot      \ (acc addr len -- )  move the accumulator above the others
    0            \ (acc addr len 0 -- )
    swap         \ (acc addr 0 len -- )

    \ count down in the iterator, so that as we iterator through the buffer,
    \ the iterator I will indicate how many bits (8 * I) we need to shift by
    \ so that the final spot in the buffer is the final byte of the number
    -DO
        dup      \ (acc addr addr -- )
        c@       \ (acc addr valueAtOffsetAddr -- )
        $7F and  \ mask out the most significant bit
        I 1 - 8 *    \ (acc addr valueAtOffsetAddr 8i -- )
        lshift   \ (acc addr shiftedValueAtOffset -- )
        rot      \ (addr shiftedValueAtOffset acc -- )
        +        \ (addr updatedAcc -- )
        swap     \ (acc addr -- )
        1 +      \ (acc offsetaddr -- )
    1 -LOOP

    drop         \ (acc -- ) drop the addrs, and we'll leave the accumulator on the stack
;