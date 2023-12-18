\ Variables:
\ ---------------------------------------------------------------------
Variable pageSize
1024 pageSize !

\ Type of B-Tree node (PGTYPE_TABLE_INTERNAL, PGTYPE_TABLE_LEAF, PGTYPE_INDEX_INTERNAL, or PGTYPE_INDEX_LEAF)
\ from spec
0x05 Constant PGTYPE_TABLE_INTERNAL
0x0D Constant PGTYPE_TABLE_LEAF
0x02 Constant PGTYPE_INDEX_INTERNAL
0x0A Constant PGTYPE_INDEX_LEAF
