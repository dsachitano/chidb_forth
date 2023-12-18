s" globalsAndConsts.fs" required
s" utils.fs" required

\ /* BTreeCell is an in-memory representation of a cell. See The chidb File Format
\  * document for more details on the meaning of each field */
\ struct BTreeCell
\ {
\     uint8_t type;  /* Type of page where this cell is contained */
\     chidb_key_t key;     /* Key */
\     union
\     {
\         struct
\         {
\             npage_t child_page;  /* Child page with keys <= key */
\         } tableInternal;
\         struct
\         {
\             uint32_t data_size;  /* Number of bytes of data stored in this cell */
\             uint8_t *data;       /* Pointer to in-memory copy of data stored in this cell */
\         } tableLeaf;
\         struct
\         {
\             chidb_key_t keyPk;         /* Primary key of row where the indexed field is equal to key */
\             npage_t child_page;  /* Child page with keys < key */
\         } indexInternal;
\         struct
\         {
\             chidb_key_t keyPk;         /* Primary key of row where the indexed field is equal to key */
\         } indexLeaf;
\     } fields;
\ };
\
\ I'll have two kinds of table cells (leave indexes for later):
\   * internal
\   * leaf
\
\ So keep 1 byte for pageType (equivalent to tableCellType) at the very start
\ then after that I can lay out the memory exactly the way it will be in the
\ blocks, to make it easy to copy-paste cells.  Then for getters and setters,
\ we just need to check the first byte to know where to read/write each field.
\ also we'll need a helper to get the offset-by-one byte addr for copy/pastes.
\

\ internal table cells are 9 bytes: 1 for type, 4 for childPageNum, and 4 for key
: allocateTableCellInternal ( -- cellAddr )
    9 allocate        ( -- addr wior)

    IF
        ." failed to allocate memory in allocateBtreeNode"
    ENDIF      

    dup             ( addr -- addr addr)
    9 32 fill       ( addr addr 9 32 -- addr)  \ initialize each byte to 0x20
    dup             ( addr -- addr addr )
    PGTYPE_TABLE_INTERNAL swap ( addr addr -- addr PGTYPE_TABLE_INTERNAL addr )
    c!
;

: allocateTableCellLeaf ( recordSize -- cellAddr )
    8 +             \ add 9 bytes to whatever the record size is:
                    \ 1 for type, 4 for recordSize, 4 for key, the rest is recordSize 
    dup             ( memSize -- memSize memSize )
    allocate        ( memSize -- memSize addr wior)

    IF
        ." failed to allocate memory in allocateBtreeNode"
    ENDIF           ( memSize addr wior -- memSize addr)

    dup             ( memSize addr -- memSize addr addr)
    rot             ( memSize addr addr -- addr addr memSize )
    32 fill         ( addr addr memSize 32 -- addr)  \ initialize each byte to 0x20
    dup             ( addr -- addr addr )
    PGTYPE_TABLE_LEAF swap  ( addr addr -- addr PGTYPE_TABLE_LEAF addr )
    c!
;


: tableCell_getType ( cellAddr -- type)
    c@
;



: tableCell_internal_setChildPageNum ( val cellAddr -- )
    1 +             \ offset into the struct ( val offsetAddr -- )
    4 writeMultiByteNum
;

: tableCell_internal_getChildPageNum ( val cellAddr -- pageNum )
    1 +             \ offset into the struct ( val offsetAddr -- )
    4 multiByteNum
;

: tableCell_internal_setKey ( val cellAddr -- )
    5 +             \ offset into the struct ( val offsetAddr -- )
    4 writeMultiByteNum
;

: tableCell_internal_getKey ( val cellAddr -- key)
    5 +             \ offset into the struct ( val offsetAddr -- )
    4 multiByteNum
;

: tableCell_leaf_getRecordSize ( cellAddr -- recordSize)
    1 +             \ offset into the struct
    4 multiByteNum
;

: tableCell_getSize ( cellAddr -- size)
    dup         ( cellAddr -- cellAddr cellAddr )
    tableCell_getType   ( cellAddr cellAddr -- cellAddr type)
    PGTYPE_TABLE_INTERNAL =
    IF
        drop
        9
    ELSE
        tableCell_leaf_getRecordSize    ( cellAddr -- recordSize)

        \ there are 9 bytes for the leaf cell (1 type, 4 size, 4 key ) + recordSize 
        9 +                             ( recordSize -- cellSize)
    ENDIF
;

\ this is like tableCell_getSize but the size of the on-disk block,
\ not the in-memory struct (should just be off by one, for the type)
: tableCell_getBlockSize ( cellAddr -- blockSize )
    tableCell_getSize 1 -
;