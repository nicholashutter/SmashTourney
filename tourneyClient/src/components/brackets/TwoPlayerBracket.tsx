import drawService from "../../services/DrawService";
import { motion } from "framer-motion";



const TwoPlayerBracket = () =>
{
    return (
        <motion.svg>
            <motion.line
                x1="220"
                y1="30"
                x2="360"
                y2="170"
                stroke="#00cc88"
                variants={drawService}
                custom={2}
            />
        </motion.svg>
    )
}

export default TwoPlayerBracket