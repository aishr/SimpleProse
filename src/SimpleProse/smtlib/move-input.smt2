(declare-const Inv_0_n (Array Int Int)) 
(declare-const Inv_1_n (Array Int Int)) 
(declare-const Inv_2_n Int)
(declare-const Inv_3_n Int)

(assert
        (or (<= (select Inv_0_n 1) 0) (not (= Inv_3_n (select Inv_1_n 1))) (< (select Inv_0_n 3) 2))
)
